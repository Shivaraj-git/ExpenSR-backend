using ExpenSR.Data;
using ExpenSR.Exceptions;
using ExpenSR.Helpers;
using ExpenSR.Models.DTOs;
using ExpenSR.Models.Entities;
using ExpenSR.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpenSR.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly ITokenService _tokenService;

        public AuthService(ApplicationDbContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        public async Task<UserResponseDTO> SignUpUserAsync(SignUpUserDTO dto)
        {
            var companyExists = await _db.Companies.AnyAsync(c => c.CompanyId == dto.CompanyId);
            if (!companyExists)
                throw new NotFoundException("Company not found.");

            var emailTaken = await _db.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailTaken)
                throw new ConflictException("An account with this email already exists.");

            var company = await _db.Companies.FirstAsync(c => c.CompanyId == dto.CompanyId);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                CompanyId = dto.CompanyId,
                Company = company,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                UserRole = dto.UserRole,
                ApprovalStatus = ApprovalStatus.Pending,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return MapToUserResponseDTO(user, managerName: null);
        }

        public async Task<AuthResponseDTO> LoginUserAsync(LoginUserDTO dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new InvalidCredentialsException();

            if (user.ApprovalStatus == ApprovalStatus.Pending)
                throw new AccountNotApprovedException("Your signup is still awaiting admin approval.");

            if (user.ApprovalStatus == ApprovalStatus.Rejected)
                throw new AccountNotApprovedException("Your signup request was rejected. Contact your admin.");

            if (!user.IsActive)
                throw new AccountNotApprovedException("This account has been deactivated.");

            var (token, expiresAt) = _tokenService.GenerateToken(
                user.UserId,
                user.Email,
                user.UserRole.ToString(), // "Employee" or "Manager"
                user.CompanyId
            );

            return new AuthResponseDTO
            {
                Token = token,
                ExpiresAt = expiresAt,
                Id = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.UserRole.ToString(),
                CompanyId = user.CompanyId
            };
        }

        public async Task<AuthResponseDTO> LoginAdminAsync(LoginAdminDTO dto)
        {
            var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Email == dto.Email);

            if (admin is null || !BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash))
                throw new InvalidCredentialsException();

            if (!admin.IsActive)
                throw new AccountNotApprovedException("This admin account has been deactivated.");

            var (token, expiresAt) = _tokenService.GenerateToken(
                admin.AdminId,
                admin.Email,
                "Admin",
                admin.CompanyId
            );

            return new AuthResponseDTO
            {
                Token = token,
                ExpiresAt = expiresAt,
                Id = admin.AdminId,
                FirstName = admin.FirstName,
                LastName = admin.LastName,
                Email = admin.Email,
                Role = "Admin",
                CompanyId = admin.CompanyId
            };
        }

        private static UserResponseDTO MapToUserResponseDTO(User user, string? managerName) => new()
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            UserRole = user.UserRole,
            ManagerId = user.ManagerId,
            ManagerName = managerName,
            ApprovalStatus = user.ApprovalStatus,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}