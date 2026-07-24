namespace ExpenSR.Models.DTOs
{
    public class RejectSignupDTO
    {
        public Guid UserId { get; set; }

        public string? Reason { get; set; }
    }
}