namespace TaskFlowAPI.Models
{
    public class TaskTimeLog
    {
        public Guid Id { get; set; }

        public Guid TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
