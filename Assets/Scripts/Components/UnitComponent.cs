using UnityEngine;
using System.Collections;

public class UnitComponent : MonoBehaviour
{
    protected Unit ParentUnit { get; private set; }
    public GameboardHelper Helper { get { return ParentUnit.Helper; } }

    public void Initialize(Unit unit)
    {
        ParentUnit = unit;
        OnInitialize();
    }

    protected virtual void OnInitialize() { }
}
