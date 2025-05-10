using TaskFlowAPI.Models.Enums;
namespace TaskFlowAPI.DTOs
{
    public class TaskDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectTitle { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
        public Guid? AssignedToId { get; set; }
        public string? AssignedTo { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string CreatedBy { get; set; } = null!;
    }
}
