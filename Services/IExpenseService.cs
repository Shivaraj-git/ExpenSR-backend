using ExpenSR.Models.DTOs;
using ExpenSR.Models.Enums;

namespace ExpenSR.Services
{
    public interface IExpenseService
    {
        // ---- Employee / Manager (own claims) ----
        Task<ExpenseResponseDTO> CreateExpenseAsync(Guid userId, Guid companyId, CreateExpenseDTO dto);

        Task<List<ExpenseResponseDTO>> GetMyExpensesAsync(Guid userId, ExpenseStatus? statusFilter);

        Task<ExpenseResponseDTO> UpdateExpenseAsync(Guid userId, Guid expenseId, UpdateExpenseDTO dto);

        Task DeleteExpenseAsync(Guid userId, Guid expenseId);

        // ---- Manager (own team's claims) ----
        Task<List<ExpenseResponseDTO>> GetPendingApprovalsAsync(Guid managerId);

        Task<List<ExpenseResponseDTO>> GetTeamExpensesAsync(Guid managerId, ExpenseStatus? statusFilter);

        Task<ExpenseResponseDTO> ReviewExpenseAsync(Guid managerId, ReviewExpenseDTO dto);

        // ---- Admin (company-wide) ----
        Task<List<ExpenseResponseDTO>> GetAllExpensesAsync(Guid companyId, ExpenseStatus? statusFilter);

        // ---- Shared ----
        /// <summary>
        /// Fetches a single expense, enforcing visibility: the owner, their manager,
        /// or an admin in the same company. Throws NotFoundException otherwise
        /// (deliberately not a 403, to avoid leaking existence of other users' claims).
        /// </summary>
        Task<ExpenseResponseDTO> GetExpenseByIdAsync(
            Guid requestingUserId, string requestingRole, Guid companyId, Guid expenseId);

        Task<List<ExpenseCategoryResponseDTO>> GetCategoriesAsync(Guid companyId);
    }
}