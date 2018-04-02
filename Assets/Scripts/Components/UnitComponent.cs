using UnityEngine;
using Framework;

public class UnitComponent : MonoBehaviour
{
    protected Unit ParentUnit { get; private set; }
    public GameboardWorldHelper Helper { get { return ParentUnit.Helper; } }

    private bool _initialized;

    public void Initialize(Unit unit)
    {
        if (_initialized)
        {
            DebugEx.LogWarning<UnitComponent>("Component was already initialized. Are you calling Initialize twice?");
            return;
        }

        _initialized = true;

        ParentUnit = unit;
        OnInitialize();
    }

    protected virtual void OnInitialize() { }
}
