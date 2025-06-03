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

            // بيانات المستخدم الأدمن الافتراضي
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Name = "Admin",
                Email = "admin@system.com",
                PasswordHash = "admin123",
                Role = "Admin",
                IsDeleted = false
            });

            // Soft Delete: فلتر افتراضي لاستبعاد المحذوفين من المستخدمين
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);

            // العلاقات بين WorkflowStep و User
            modelBuilder.Entity<WorkflowStep>()
                .HasOne(w => w.User)
                .WithMany(u => u.WorkflowSteps)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict); // منع حذف المستخدم لو مرتبط بخطوات الموافقة

            // العلاقات بين WorkflowStep و FileItem
            modelBuilder.Entity<WorkflowStep>()
                .HasOne(w => w.FileItem)
                .WithMany(f => f.WorkflowSteps)
                .HasForeignKey(w => w.FileItemId)
                .OnDelete(DeleteBehavior.Cascade); // حذف خطوات الملف مع الملف نفسه

            // العلاقة بين FileItem و CreatedBy (المستخدم الذي أنشأ الملف)
            modelBuilder.Entity<FileItem>()
                .HasOne(f => f.CreatedBy)
                .WithMany()
                .HasForeignKey(f => f.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // العلاقة بين FileItem و SentToUser (المستخدم الحالي المطلوب منه الموافقة)
            modelBuilder.Entity<FileItem>()
                .HasOne(f => f.SentToUser)
                .WithMany()
                .HasForeignKey(f => f.SentToUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
