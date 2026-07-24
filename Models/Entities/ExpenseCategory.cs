using System.ComponentModel.DataAnnotations;

namespace ExpenSR.Models.Entities
{
    public class ExpenseCategory
    {
        [Key]
        public Guid CategoryId { get; set; }

        public Guid CompanyId { get; set; }

        public required Company Company { get; set; }

        [Required]
        public required string CategoryName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}