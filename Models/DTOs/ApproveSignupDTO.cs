using ExpenSR.Models.Enums;

namespace ExpenSR.Models.DTOs
{
    public class ApproveSignupDTO
    {
        public Guid UserId { get; set; }

        // Admin can confirm or override the role the user picked at signup
        public UserRole UserRole { get; set; }

        // Required if UserRole == Employee. A Manager may or may not have their own manager.
        public Guid? ManagerId { get; set; }
    }
}