using UnityEngine;
using Framework;

public class UnitComponent : MonoBehaviour
{
    public Unit Unit { get; private set; }
    public Helper Helper { get { return Unit.Helper; } }

    private bool _initialized;

    public void Initialize(Unit unit)
    {
        if (_initialized)
        {
            DebugEx.LogWarning<UnitComponent>("Component was already initialized. Are you calling Initialize twice?");
            return;
        }

        _initialized = true;

        Unit = unit;
        OnInitialize();
    }

    protected virtual void OnInitialize() { }
}
