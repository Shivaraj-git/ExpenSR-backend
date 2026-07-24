using ExpenSR.Models.DTOs;

namespace ExpenSR.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user with ApprovalStatus.Pending.
        /// The user cannot log in until an Admin approves the signup.
        /// </summary>
        Task<UserResponseDTO> SignUpUserAsync(SignUpUserDTO dto);

        /// <summary>
        /// Authenticates an Employee or Manager. Fails if the account
        /// is not yet approved, was rejected, or is inactive.
        /// </summary>
        Task<AuthResponseDTO> LoginUserAsync(LoginUserDTO dto);

        /// <summary>
        /// Authenticates an Admin.
        /// </summary>
        Task<AuthResponseDTO> LoginAdminAsync(LoginAdminDTO dto);
    }
}