namespace TaskFlowAPI.DTOs
{
    public class CreateTaskDto
    {
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        //public Guid? AssignedToId { get; set; } // optional
    }
}
