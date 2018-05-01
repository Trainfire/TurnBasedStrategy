public class StateFlags : IReadOnlyStateFlags
{
    public bool CanContinue { get; set; }
    public bool CanControlUnits { get; set; }
    public bool CanSpawnUnits { get; set; }
    public bool CanUndo { get; set; }

    public void Clear()
    {
        CanContinue = false;
        CanControlUnits = false;
        CanSpawnUnits = false;
        CanUndo = false;
    }
}

public interface IReadOnlyStateFlags
{
    bool CanContinue { get; }
    bool CanControlUnits { get; }
    bool CanSpawnUnits { get; }
    bool CanUndo { get; }
}