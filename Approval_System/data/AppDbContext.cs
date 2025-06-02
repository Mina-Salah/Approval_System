using Approval_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Approval_System.data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<FileItem> FileItems { get; set; }
        public DbSet<WorkflowStep> WorkflowSteps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Admin ثابت
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Name = "Admin",
                Email = "admin@system.com",
                PasswordHash = "admin123",
                Role = "Admin"
            });
    
            // منع الحذف المتسلسل من WorkflowStep → User
            modelBuilder.Entity<WorkflowStep>()
                .HasOne(w => w.User)
                .WithMany(u => u.WorkflowSteps)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict); // 👈 هذا هو المفتاح

            // منع الحذف المتسلسل من WorkflowStep → FileItem
            modelBuilder.Entity<WorkflowStep>()
                .HasOne(w => w.FileItem)
                .WithMany(f => f.WorkflowSteps)
                .HasForeignKey(w => w.FileItemId)
                .OnDelete(DeleteBehavior.Cascade); // مسموح بواحدة فقط تكون Cascade

            // نفس الشيء مع FileItem → CreatedBy
            modelBuilder.Entity<FileItem>()
                .HasOne(f => f.CreatedBy)
                .WithMany()
                .HasForeignKey(f => f.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة الموظف المستلم
            modelBuilder.Entity<FileItem>()
                .HasOne(f => f.SentToUser)
                .WithMany()
                .HasForeignKey(f => f.SentToUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }

}
