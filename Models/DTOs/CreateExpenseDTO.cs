namespace ExpenSR.Models.DTOs
{
    public class CreateExpenseDTO
    {
        public Guid CategoryId { get; set; }

        public required string Description { get; set; }

        public decimal Amount { get; set; }

        public DateTime ExpenseDate { get; set; }
    }
}