using TaskFlowAPI.Interfaces;
using TaskFlowAPI.Models.Enums;

namespace TaskFlowAPI.Models
{
    public class TaskItem : IAuditableEntity
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public TaskItemStatus Status { get; set; } = TaskItemStatus.ToDo;

        public Guid ProjectId { get; set; }
        public Project Project { get; set; }

        public Guid? AssignedToId { get; set; }
        public User? AssignedTo { get; set; }

        public Guid CreatedById { get; set; }
        public User CreatedBy { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public Guid? UpdatedById { get; set; }
        public User? UpdatedBy { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public ICollection<TaskTimeLog> TimeLogs { get; set; }
    }
}
