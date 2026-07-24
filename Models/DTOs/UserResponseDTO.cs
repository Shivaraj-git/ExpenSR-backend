using ExpenSR.Models.Enums;

namespace ExpenSR.Models.DTOs
{
    public class UserResponseDTO
    {
        public Guid UserId { get; set; }

        public required string FirstName { get; set; }

        public string? LastName { get; set; }

        public required string Email { get; set; }

        public UserRole UserRole { get; set; }

        public Guid? ManagerId { get; set; }

        public string? ManagerName { get; set; }

        public ApprovalStatus ApprovalStatus { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}