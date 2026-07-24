namespace ExpenSR.Models.DTOs
{
    public class AssignManagerDTO
    {
        public Guid UserId { get; set; }

        // Null is allowed - e.g. clearing a Manager's own manager,
        // or un-assigning an employee temporarily
        public Guid? ManagerId { get; set; }
    }
}