namespace ExpenSR.Models.DTOs
{
    public class AuthResponseDTO
    {
        public required string Token { get; set; }

        public DateTime ExpiresAt { get; set; }

        public Guid Id { get; set; }

        public required string FirstName { get; set; }

        public string? LastName { get; set; }

        public required string Email { get; set; }

        // "Admin", "Employee", or "Manager"
        public required string Role { get; set; }

        public Guid CompanyId { get; set; }
    }
}