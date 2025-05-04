using System.ComponentModel.DataAnnotations;
using TaskFlowAPI.Interfaces;

namespace TaskFlowAPI.Models
{
    public class Project : IAuditableEntity
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public Guid CreatedById { get; set; }
        public User CreatedBy { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public Guid? UpdatedById { get; set; }
        public User? UpdatedBy { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public ICollection<ProjectUser> ProjectUsers { get; set; }
        public ICollection<TaskItem> Tasks { get; set; }
    }
}
