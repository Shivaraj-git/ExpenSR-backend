using ExpenSR.Data;
using ExpenSR.Exceptions;
using ExpenSR.Models.DTOs;
using ExpenSR.Models.Entities;
using ExpenSR.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpenSR.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _db;

        public UserService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<UserResponseDTO> CreateUserAsync(Guid companyId, CreateUserDTO dto)
        {
            var emailTaken = await _db.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailTaken)
                throw new ConflictException("An account with this email already exists.");

            var manager = await ValidateAndFetchManagerAsync(companyId, dto.UserRole, dto.ManagerId);

            var company = await _db.Companies.FirstAsync(c => c.CompanyId == companyId);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                CompanyId = companyId,
                Company = company,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                UserRole = dto.UserRole,
                ManagerId = manager?.UserId,
                ApprovalStatus = ApprovalStatus.Approved, // admin-created users skip the signup gate
                ApprovedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return MapToDTO(user, manager);
        }

        public async Task<List<UserResponseDTO>> GetPendingSignupsAsync(Guid companyId)
        {
            var users = await _db.Users
                .Where(u => u.CompanyId == companyId && u.ApprovalStatus == ApprovalStatus.Pending)
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();

            return users.Select(u => MapToDTO(u, manager: null)).ToList();
        }

        public async Task<UserResponseDTO> ApproveSignupAsync(Guid companyId, Guid approvingAdminId, ApproveSignupDTO dto)
        {
            var user = await GetUserInCompanyOrThrow(companyId, dto.UserId);

            if (user.ApprovalStatus != ApprovalStatus.Pending)
                throw new ConflictException("This signup has already been reviewed.");

            var manager = await ValidateAndFetchManagerAsync(companyId, dto.UserRole, dto.ManagerId, excludeUserId: user.UserId);

            user.UserRole = dto.UserRole;
            user.ManagerId = manager?.UserId;
            user.ApprovalStatus = ApprovalStatus.Approved;
            user.ApprovedByAdminId = approvingAdminId;
            user.ApprovedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return MapToDTO(user, manager);
        }

        public async Task<UserResponseDTO> RejectSignupAsync(Guid companyId, Guid approvingAdminId, RejectSignupDTO dto)
        {
            var user = await GetUserInCompanyOrThrow(companyId, dto.UserId);

            if (user.ApprovalStatus != ApprovalStatus.Pending)
                throw new ConflictException("This signup has already been reviewed.");

            user.ApprovalStatus = ApprovalStatus.Rejected;
            user.ApprovedByAdminId = approvingAdminId;
            user.ApprovedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return MapToDTO(user, manager: null);
        }

        public async Task<UserResponseDTO> AssignManagerAsync(Guid companyId, AssignManagerDTO dto)
        {
            var user = await GetUserInCompanyOrThrow(companyId, dto.UserId);

            User? manager = null;
            if (dto.ManagerId.HasValue)
            {
                if (dto.ManagerId.Value == user.UserId)
                    throw new ValidationException("A user cannot be their own manager.");

                manager = await GetUserInCompanyOrThrow(companyId, dto.ManagerId.Value);

                if (manager.UserRole != UserRole.Manager)
                    throw new ValidationException("Assigned manager must have the Manager role.");

                // prevent a simple two-hop cycle: manager reporting to the user being updated
                if (manager.ManagerId == user.UserId)
                    throw new ValidationException("This assignment would create a circular reporting relationship.");
            }
            else if (user.UserRole == UserRole.Employee)
            {
                // Employees must always have a manager - only Managers may have ManagerId == null
                throw new ValidationException("Employees must be assigned a manager. To remove this employee's manager, assign a new one instead.");
            }

            user.ManagerId = manager?.UserId;
            await _db.SaveChangesAsync();

            return MapToDTO(user, manager);
        }

        public async Task<UserResponseDTO> ChangeRoleAsync(Guid companyId, ChangeRoleDTO dto)
        {
            var user = await GetUserInCompanyOrThrow(companyId, dto.UserId);

            if (user.UserRole == UserRole.Manager && dto.NewRole == UserRole.Employee)
            {
                var hasReports = await _db.Users.AnyAsync(u => u.ManagerId == user.UserId);
                if (hasReports)
                    throw new ConflictException(
                        "This user still has employees reporting to them. Reassign those employees before demoting.");

                // Employees must always have a manager. A Manager being demoted may
                // currently have ManagerId == null (Managers aren't required to have one),
                // which would leave the demoted Employee in an invalid state.
                if (!user.ManagerId.HasValue)
                    throw new ConflictException(
                        "This user has no manager assigned. Assign a manager before demoting them to Employee.");
            }

            user.UserRole = dto.NewRole;

            // an Employee promoted to Manager keeps their own manager unless the admin changes it separately
            await _db.SaveChangesAsync();

            User? manager = user.ManagerId.HasValue
                ? await _db.Users.FirstOrDefaultAsync(u => u.UserId == user.ManagerId)
                : null;

            return MapToDTO(user, manager);
        }

        public async Task<List<UserResponseDTO>> GetAllUsersAsync(Guid companyId)
        {
            var users = await _db.Users
                .Include(u => u.Manager)
                .Where(u => u.CompanyId == companyId)
                .OrderBy(u => u.FirstName)
                .ToListAsync();

            return users.Select(u => MapToDTO(u, u.Manager)).ToList();
        }

        public async Task<List<UserResponseDTO>> GetManagersAsync(Guid companyId)
        {
            var managers = await _db.Users
                .Where(u => u.CompanyId == companyId
                    && u.UserRole == UserRole.Manager
                    && u.ApprovalStatus == ApprovalStatus.Approved
                    && u.IsActive)
                .OrderBy(u => u.FirstName)
                .ToListAsync();

            return managers.Select(u => MapToDTO(u, manager: null)).ToList();
        }

        // ---- helpers ----

        private async Task<User> GetUserInCompanyOrThrow(Guid companyId, Guid userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.CompanyId == companyId);
            if (user is null)
                throw new NotFoundException("User not found.");
            return user;
        }

        /// <summary>
        /// Employees must be assigned a Manager; Managers may optionally have one.
        /// Validates the referenced manager belongs to the same company and holds the Manager role.
        /// </summary>
        private async Task<User?> ValidateAndFetchManagerAsync(
            Guid companyId, UserRole role, Guid? managerId, Guid? excludeUserId = null)
        {
            if (role == UserRole.Employee && !managerId.HasValue)
                throw new ValidationException("Employees must be assigned a manager.");

            if (!managerId.HasValue)
                return null;

            if (excludeUserId.HasValue && managerId.Value == excludeUserId.Value)
                throw new ValidationException("A user cannot be their own manager.");

            var manager = await _db.Users.FirstOrDefaultAsync(
                u => u.UserId == managerId.Value && u.CompanyId == companyId);

            if (manager is null)
                throw new NotFoundException("Assigned manager not found in this company.");

            if (manager.UserRole != UserRole.Manager)
                throw new ValidationException("Assigned manager must have the Manager role.");

            return manager;
        }

        private static UserResponseDTO MapToDTO(User user, User? manager) => new()
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            UserRole = user.UserRole,
            ManagerId = user.ManagerId,
            ManagerName = manager is null ? null : $"{manager.FirstName} {manager.LastName}".Trim(),
            ApprovalStatus = user.ApprovalStatus,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}