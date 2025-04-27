namespace TaskFlowAPI.DTOs
{
    public class CreateProjectDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
    }
}
