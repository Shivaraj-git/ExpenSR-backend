using ExpenSR.Models.DTOs;

namespace ExpenSR.Services
{
    public interface IUserService
    {
        /// <summary>Admin creates an Employee or Manager directly - auto-approved, no signup gate.</summary>
        Task<UserResponseDTO> CreateUserAsync(Guid companyId, CreateUserDTO dto);

        Task<List<UserResponseDTO>> GetPendingSignupsAsync(Guid companyId);

        Task<UserResponseDTO> ApproveSignupAsync(Guid companyId, Guid approvingAdminId, ApproveSignupDTO dto);

        Task<UserResponseDTO> RejectSignupAsync(Guid companyId, Guid approvingAdminId, RejectSignupDTO dto);

        Task<UserResponseDTO> AssignManagerAsync(Guid companyId, AssignManagerDTO dto);

        Task<UserResponseDTO> ChangeRoleAsync(Guid companyId, ChangeRoleDTO dto);

        Task<List<UserResponseDTO>> GetAllUsersAsync(Guid companyId);

        /// <summary>Users with UserRole == Manager, for populating manager-assignment dropdowns.</summary>
        Task<List<UserResponseDTO>> GetManagersAsync(Guid companyId);
    }
}