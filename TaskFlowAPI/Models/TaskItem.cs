using TaskFlowAPI.Models.Enums;

namespace TaskFlowAPI.Models
{
    public class TaskItem
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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TaskTimeLog> TimeLogs { get; set; }
    }
}
