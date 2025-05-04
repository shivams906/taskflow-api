namespace TaskFlowAPI.Interfaces
{
    public interface ICurrentSessionProvider
    {
        Guid? GetUserId();
    }
}
