namespace TaskFlowAPI.DTOs
{
    public class TimeLogDto
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid UserId { get; set;  }
        public string Username { get; set; } = null!;
    }
}
