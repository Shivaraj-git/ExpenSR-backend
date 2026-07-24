using ExpenSR.Models.Enums;

namespace ExpenSR.Models.DTOs
{
    public class ExpenseResponseDTO
    {
        public Guid ExpenseId { get; set; }

        public Guid UserId { get; set; }

        public required string UserName { get; set; }

        public Guid CategoryId { get; set; }

        public required string CategoryName { get; set; }

        public required string Description { get; set; }

        public decimal Amount { get; set; }

        public DateTime ExpenseDate { get; set; }

        public ExpenseStatus Status { get; set; }

        public Guid? ReviewedByManagerId { get; set; }

        public string? ReviewedByManagerName { get; set; }

        public string? ManagerComment { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}