using System.ComponentModel.DataAnnotations;

namespace Approval_System.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Role { get; set; } = "User";
        public bool IsDeleted { get; set; } // جديد

        public ICollection<WorkflowStep> WorkflowSteps { get; set; }
    }

}
