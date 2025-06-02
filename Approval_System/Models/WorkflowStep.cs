namespace Approval_System.Models
{
    public class WorkflowStep
    {
        public int Id { get; set; }

        public int FileItemId { get; set; }
        public FileItem FileItem { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int Order { get; set; } // ترتيب الموافقة

        public bool IsApproved { get; set; } = false;
        public bool IsRejected { get; set; }

        public DateTime? ApprovedAt { get; set; } // وقت الموافقة

        public string Action { get; set; } = "قيد الانتظار";
    }

}
