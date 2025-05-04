using TaskFlowAPI.Interfaces;

namespace TaskFlowAPI.Models
{
    public class TaskTimeLog : IAuditableEntity
    {
        public Guid Id { get; set; }

        public Guid TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public Guid CreatedById { get; set; }
        public User CreatedBy { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public Guid? UpdatedById { get; set; }
        public User? UpdatedBy { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
