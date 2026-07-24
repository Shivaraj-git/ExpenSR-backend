using System.ComponentModel.DataAnnotations;

namespace ExpenSR.Models.Entities
{
    public class Admin
    {
        [Key]
        public Guid AdminId { get; set; }

        public Guid CompanyId { get; set; }

        public required Company Company { get; set; }

        public required string FirstName { get; set; }

        public string? LastName { get; set; }

        public required string Email { get; set; }

        public required string PasswordHash { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}