using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectUser> ProjectUsers => Set<ProjectUser>();
        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<TaskTimeLog> TaskTimeLogs => Set<TaskTimeLog>();
        public DbSet<AuditLog> AuditLogs { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProjectUser>()
                .HasKey(pu => new { pu.ProjectId, pu.UserId });

            modelBuilder.Entity<ProjectUser>()
                .HasOne(pu => pu.User)
                .WithMany(u => u.ProjectUsers)
                .HasForeignKey(pu => pu.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectUser>()
                .HasOne(pu => pu.Project)
                .WithMany(p => p.ProjectUsers)
                .HasForeignKey(pu => pu.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectUser>()
                .HasOne(pu => pu.CreatedBy)
                .WithMany()
                .HasForeignKey(pu => pu.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectUser>()
                .HasOne(pu => pu.UpdatedBy)
                .WithMany()
                .HasForeignKey(pu => pu.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectUser>()
                .Property(p => p.Role)
                .HasConversion<string>();

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.AssignedTo)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.CreatedBy)
                .WithMany()
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskItem>()
                .Property(t => t.Status)
                .HasConversion<string>();

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.AuditType)
                .HasConversion<string>();

            modelBuilder.Entity<TaskTimeLog>()
                .HasOne(tt => tt.User)
                .WithMany()
                .HasForeignKey(tt => tt.UserId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
