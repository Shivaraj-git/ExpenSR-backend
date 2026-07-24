namespace ExpenSR.Models.DTOs
{
    public class ExpenseCategoryResponseDTO
    {
        public Guid CategoryId { get; set; }

        public required string CategoryName { get; set; }
    }
}