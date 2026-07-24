using ExpenSR.Models.Enums;

namespace ExpenSR.Models.DTOs
{
    public class ChangeRoleDTO
    {
        public Guid UserId { get; set; }

        public UserRole NewRole { get; set; }
    }
}