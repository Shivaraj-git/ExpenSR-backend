using ExpenSR.Models.Enums;

namespace ExpenSR.Models.DTOs
{
    public class SignUpUserDTO
    {
        public Guid CompanyId { get; set; }

        public required string FirstName { get; set; }

        public string? LastName { get; set; }

        public required string Email { get; set; }

        public required string Password { get; set; }

        public UserRole UserRole { get; set; }

    }
}
