public interface IReadOnlyStateFlags
{
    bool CanContinue { get; }
    bool CanSelectedUnitAttack { get; }
    bool CanSelectedUnitMove { get; }
    bool CanSelectedUnitRepair { get; }
    bool CanSpawnUnits { get; }
    bool CanUndo { get; }
}

public class StateFlags : IReadOnlyStateFlags
{
    public bool CanContinue { get; set; }
    public bool CanSelectedUnitAttack { get; set; }
    public bool CanSelectedUnitMove { get; set; }
    public bool CanSelectedUnitRepair { get; set; }
    public bool CanSpawnUnits { get; set; }
    public bool CanUndo { get; set; }

    public void Clear()
    {
        CanContinue = false;
        CanSelectedUnitAttack = false;
        CanSelectedUnitMove = false;
        CanSelectedUnitRepair = false;
        CanSpawnUnits = false;
        CanUndo = false;
    }
}