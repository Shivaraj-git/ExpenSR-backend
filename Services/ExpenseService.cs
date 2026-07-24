using ExpenSR.Data;
using ExpenSR.Exceptions;
using ExpenSR.Models.DTOs;
using ExpenSR.Models.Entities;
using ExpenSR.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpenSR.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly ApplicationDbContext _db;

        public ExpenseService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ---------------- Employee / Manager: own claims ----------------

        public async Task<ExpenseResponseDTO> CreateExpenseAsync(Guid userId, Guid companyId, CreateExpenseDTO dto)
        {
            if (dto.Amount <= 0)
                throw new ValidationException("Amount must be greater than zero.");

            if (dto.ExpenseDate.Date > DateTime.UtcNow.Date)
                throw new ValidationException("Expense date cannot be in the future.");

            var category = await _db.ExpenseCategories.FirstOrDefaultAsync(
                c => c.CategoryId == dto.CategoryId && c.CompanyId == companyId);

            if (category is null)
                throw new NotFoundException("Expense category not found.");

            if (!category.IsActive)
                throw new ValidationException("This expense category is no longer active.");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId)
                ?? throw new NotFoundException("User not found.");

            var expense = new Expense
            {
                ExpenseId = Guid.NewGuid(),
                UserId = userId,
                User = user,
                CategoryId = category.CategoryId,
                Category = category,
                Description = dto.Description,
                Amount = dto.Amount,
                ExpenseDate = dto.ExpenseDate,
                Status = ExpenseStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _db.Expenses.Add(expense);
            await _db.SaveChangesAsync();

            return MapToDTO(expense, user, category, reviewedByManager: null);
        }

        public async Task<List<ExpenseResponseDTO>> GetMyExpensesAsync(Guid userId, ExpenseStatus? statusFilter)
        {
            var query = _db.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .Include(e => e.ReviewedByManager)
                .Where(e => e.UserId == userId);

            if (statusFilter.HasValue)
                query = query.Where(e => e.Status == statusFilter.Value);

            var expenses = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();

            return expenses.Select(e => MapToDTO(e, e.User, e.Category, e.ReviewedByManager)).ToList();
        }

        public async Task<ExpenseResponseDTO> UpdateExpenseAsync(Guid userId, Guid expenseId, UpdateExpenseDTO dto)
        {
            var expense = await _db.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.ExpenseId == expenseId);

            if (expense is null || expense.UserId != userId)
                throw new NotFoundException("Expense not found.");

            if (expense.Status != ExpenseStatus.Pending)
                throw new ConflictException("Only pending claims can be edited.");

            if (dto.Amount <= 0)
                throw new ValidationException("Amount must be greater than zero.");

            if (dto.ExpenseDate.Date > DateTime.UtcNow.Date)
                throw new ValidationException("Expense date cannot be in the future.");

            var category = await _db.ExpenseCategories.FirstOrDefaultAsync(
                c => c.CategoryId == dto.CategoryId && c.CompanyId == expense.User.CompanyId);

            if (category is null)
                throw new NotFoundException("Expense category not found.");

            if (!category.IsActive)
                throw new ValidationException("This expense category is no longer active.");

            expense.CategoryId = category.CategoryId;
            expense.Category = category;
            expense.Description = dto.Description;
            expense.Amount = dto.Amount;
            expense.ExpenseDate = dto.ExpenseDate;

            await _db.SaveChangesAsync();

            return MapToDTO(expense, expense.User, category, reviewedByManager: null);
        }

        public async Task DeleteExpenseAsync(Guid userId, Guid expenseId)
        {
            var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.ExpenseId == expenseId);

            if (expense is null || expense.UserId != userId)
                throw new NotFoundException("Expense not found.");

            if (expense.Status != ExpenseStatus.Pending)
                throw new ConflictException("Only pending claims can be deleted.");

            _db.Expenses.Remove(expense);
            await _db.SaveChangesAsync();
        }

        // ---------------- Manager: team claims ----------------

        public async Task<List<ExpenseResponseDTO>> GetPendingApprovalsAsync(Guid managerId)
        {
            var expenses = await _db.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .Include(e => e.ReviewedByManager)
                .Where(e => e.User.ManagerId == managerId && e.Status == ExpenseStatus.Pending)
                .OrderBy(e => e.ExpenseDate)
                .ToListAsync();

            return expenses.Select(e => MapToDTO(e, e.User, e.Category, e.ReviewedByManager)).ToList();
        }

        public async Task<List<ExpenseResponseDTO>> GetTeamExpensesAsync(Guid managerId, ExpenseStatus? statusFilter)
        {
            var query = _db.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .Include(e => e.ReviewedByManager)
                .Where(e => e.User.ManagerId == managerId);

            if (statusFilter.HasValue)
                query = query.Where(e => e.Status == statusFilter.Value);

            var expenses = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();

            return expenses.Select(e => MapToDTO(e, e.User, e.Category, e.ReviewedByManager)).ToList();
        }

        public async Task<ExpenseResponseDTO> ReviewExpenseAsync(Guid managerId, ReviewExpenseDTO dto)
        {
            var expense = await _db.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.ExpenseId == dto.ExpenseId);

            // Not found OR this manager doesn't manage the claim's owner -> 404,
            // don't leak existence of claims outside this manager's team
            if (expense is null || expense.User.ManagerId != managerId)
                throw new NotFoundException("Expense not found.");

            if (expense.Status != ExpenseStatus.Pending)
                throw new ConflictException("This claim has already been reviewed.");

            if (!dto.Approve && string.IsNullOrWhiteSpace(dto.Comment))
                throw new ValidationException("A comment is required when rejecting a claim.");

            var manager = await _db.Users.FirstOrDefaultAsync(u => u.UserId == managerId)
                ?? throw new NotFoundException("Manager not found.");

            expense.Status = dto.Approve ? ExpenseStatus.Approved : ExpenseStatus.Rejected;
            expense.ReviewedByManagerId = managerId;
            expense.ManagerComment = dto.Comment;
            expense.ReviewedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return MapToDTO(expense, expense.User, expense.Category, manager);
        }

        // ---------------- Admin: company-wide ----------------

        public async Task<List<ExpenseResponseDTO>> GetAllExpensesAsync(Guid companyId, ExpenseStatus? statusFilter)
        {
            var query = _db.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .Include(e => e.ReviewedByManager)
                .Where(e => e.User.CompanyId == companyId);

            if (statusFilter.HasValue)
                query = query.Where(e => e.Status == statusFilter.Value);

            var expenses = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();

            return expenses.Select(e => MapToDTO(e, e.User, e.Category, e.ReviewedByManager)).ToList();
        }

        // ---------------- Shared ----------------

        public async Task<ExpenseResponseDTO> GetExpenseByIdAsync(
            Guid requestingUserId, string requestingRole, Guid companyId, Guid expenseId)
        {
            var expense = await _db.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .Include(e => e.ReviewedByManager)
                .FirstOrDefaultAsync(e => e.ExpenseId == expenseId);

            if (expense is null || expense.User.CompanyId != companyId)
                throw new NotFoundException("Expense not found.");

            var isOwner = expense.UserId == requestingUserId;
            var isManagerOfOwner = requestingRole == "Manager" && expense.User.ManagerId == requestingUserId;
            var isAdmin = requestingRole == "Admin";

            if (!isOwner && !isManagerOfOwner && !isAdmin)
                throw new NotFoundException("Expense not found.");

            return MapToDTO(expense, expense.User, expense.Category, expense.ReviewedByManager);
        }

        public async Task<List<ExpenseCategoryResponseDTO>> GetCategoriesAsync(Guid companyId)
        {
            var categories = await _db.ExpenseCategories
                .Where(c => c.CompanyId == companyId && c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return categories.Select(c => new ExpenseCategoryResponseDTO
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName
            }).ToList();
        }

        // ---------------- helpers ----------------

        private static ExpenseResponseDTO MapToDTO(
            Expense expense, User user, ExpenseCategory category, User? reviewedByManager) => new()
            {
                ExpenseId = expense.ExpenseId,
                UserId = expense.UserId,
                UserName = $"{user.FirstName} {user.LastName}".Trim(),
                CategoryId = expense.CategoryId,
                CategoryName = category.CategoryName,
                Description = expense.Description,
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                Status = expense.Status,
                ReviewedByManagerId = expense.ReviewedByManagerId,
                ReviewedByManagerName = reviewedByManager is null
                ? null
                : $"{reviewedByManager.FirstName} {reviewedByManager.LastName}".Trim(),
                ManagerComment = expense.ManagerComment,
                ReviewedAt = expense.ReviewedAt,
                CreatedAt = expense.CreatedAt
            };
    }
}