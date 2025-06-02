using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Approval_System.Models;

namespace Approval_System.ViewModel
{
    public class CreateFileViewModel
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public List<int> SelectedUserIds { get; set; } = new();

        public List<User> AllUsers { get; set; } = new();

        [Required]
        public IFormFile UploadedFile { get; set; }

        public Dictionary<int, int> UserOrders { get; set; } = new(); // userId => order
    }
}
