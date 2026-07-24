using ExpenSR.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ExpenSR.Models.Entities
{
    public class Expense
    {
        [Key]
        public Guid ExpenseId { get; set; }

        public Guid UserId { get; set; }

        public required User User { get; set; }

        public Guid CategoryId { get; set; }

        public required ExpenseCategory Category { get; set; }

        [Required]
        public required string Description { get; set; }

        public decimal Amount { get; set; }

        public DateTime ExpenseDate { get; set; }

        public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;

        // NEW: review trail - set when a Manager approves/rejects
        public Guid? ReviewedByManagerId { get; set; }

        public User? ReviewedByManager { get; set; }

        public string? ManagerComment { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}