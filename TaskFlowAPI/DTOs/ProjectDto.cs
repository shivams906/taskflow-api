namespace TaskFlowAPI.DTOs
{
    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string CreatedBy { get; set; } // Username or email

        public List<AdminDto> Admins { get; set; } = new();
    }

    public class AdminDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = null!;
    }
}
