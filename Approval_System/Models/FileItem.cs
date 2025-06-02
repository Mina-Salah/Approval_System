using System.ComponentModel.DataAnnotations;

namespace Approval_System.Models
{
    public class FileItem
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } // اسم الملف

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } // تاريخ الإرسال

        public int CreatedById { get; set; }
        public User CreatedBy { get; set; } // الموظف المنشئ

        public int? SentToUserId { get; set; } // الموظف الحالي المستلم
        public User SentToUser { get; set; }

        public string Status { get; set; } = "قيد الانتظار"; // أو "موافق", "مرفوض"

        public string FilePath { get; set; } // مسار الملف الفعلي (لو في تخزين مرفقات)

        public ICollection<WorkflowStep> WorkflowSteps { get; set; }
    }

}
