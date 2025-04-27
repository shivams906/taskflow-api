using System.ComponentModel.DataAnnotations;

namespace TaskFlowAPI.Models
{
    public class Project
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public Guid CreatedById { get; set; }
        public User CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProjectUser> ProjectUsers { get; set; }
        public ICollection<TaskItem> Tasks { get; set; }
    }
}
