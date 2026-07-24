namespace ExpenSR.Models.DTOs
{
    public class ReviewExpenseDTO
    {
        public Guid ExpenseId { get; set; }

        public bool Approve { get; set; }

        // Optional on approve, required on reject (enforced in service)
        public string? Comment { get; set; }
    }
}