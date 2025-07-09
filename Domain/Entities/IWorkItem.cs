namespace Domain.Entities
{
    public interface IWorkItem
    {
        string GetStatus();
        int GetEffortPoints();
    }
}