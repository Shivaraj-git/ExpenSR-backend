using System.ComponentModel.DataAnnotations;

namespace ExpenSR.Models.Entities
{
    public class Company
    {
        [Key]
        public Guid CompanyId { get; set; }

        [Required]
        public required string CompanyName { get; set; }

        [Required]
        public required string Country { get; set; }

        [Required]
        public required string DefaultCurrency { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}