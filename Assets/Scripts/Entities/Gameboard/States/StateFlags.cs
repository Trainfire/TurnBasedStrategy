public class StateFlags
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

    public ReadOnlyStateFlags AsReadOnly()
    {
        return new ReadOnlyStateFlags(this);
    }
}

public struct ReadOnlyStateFlags
{
    public readonly bool CanContinue;
    public readonly bool CanControlUnits;
    public readonly bool CanSpawnUnits;
    public readonly bool CanUndo;

    public ReadOnlyStateFlags(StateFlags validPlayerActions)
    {
        CanContinue = validPlayerActions.CanContinue;
        CanControlUnits = validPlayerActions.CanControlUnits;
        CanSpawnUnits = validPlayerActions.CanSpawnUnits;
        CanUndo = validPlayerActions.CanUndo;
    }
}