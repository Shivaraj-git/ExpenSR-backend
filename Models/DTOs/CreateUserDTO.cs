using ExpenSR.Models.Enums;

namespace ExpenSR.Models.DTOs
{
    public class CreateUserDTO
    {
        public required string FirstName { get; set; }

        public string? LastName { get; set; }

        public required string Email { get; set; }

        public required string Password { get; set; }

        public UserRole UserRole { get; set; }

        // Optional at creation time - required only if UserRole == Employee
        public Guid? ManagerId { get; set; }
    }
}