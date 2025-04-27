namespace TaskFlowAPI.DTOs
{
    public class AddProjectAdminDto
    {
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
    }
}
