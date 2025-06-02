using System.ComponentModel.DataAnnotations;

namespace Approval_System.ViewModel
{
    public class CreateUserViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
