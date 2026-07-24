using ExpenSR.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ExpenSR.Models.Entities
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        public Guid CompanyId { get; set; }

        public required Company Company { get; set; }

        [Required]
        public required string FirstName { get; set; }

        public string? LastName { get; set; }

        [Required]
        public required string Email { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        public UserRole UserRole { get; set; }

        // Foreign Key to Manager (another User)
        public Guid? ManagerId { get; set; }

        // Employee's Manager
        public User? Manager { get; set; }

        // Employees managed by this User
        public ICollection<User> Employees { get; set; } = new List<User>();

        // NEW: tracks whether this signup has been reviewed by an Admin.
        // Users land here as Pending after self-signup, or Approved
        // immediately if created directly by an Admin.
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

        // NEW: audit trail for who approved/rejected and when
        public Guid? ApprovedByAdminId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // IsActive is for enabling/disabling an already-approved account
        // (distinct from ApprovalStatus, which governs the initial signup gate)
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}