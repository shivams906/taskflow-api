using TaskFlowAPI.Models;

namespace TaskFlowAPI.Interfaces
{
    public interface IAuditableEntity
    {
        DateTime CreatedAtUtc { get; set; }

        DateTime? UpdatedAtUtc { get; set; }

        Guid CreatedById { get; set; }
        User CreatedBy { get; set; }
        Guid? UpdatedById { get; set; }
        User? UpdatedBy { get; set; }
    }

}
