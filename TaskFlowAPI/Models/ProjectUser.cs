using TaskFlowAPI.Interfaces;
using TaskFlowAPI.Models.Enums;

namespace TaskFlowAPI.Models
{
    public class ProjectUser : IAuditableEntity
    {
        public Guid ProjectId { get; set; }
        public Project Project { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid CreatedById { get; set; }
        public User CreatedBy { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public Guid? UpdatedById { get; set; }
        public User? UpdatedBy { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public ProjectRole Role { get; set; } = ProjectRole.Admin;
    }

}
