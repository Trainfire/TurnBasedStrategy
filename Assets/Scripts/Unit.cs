using UnityEngine;
using UnityEngine.Assertions;
using System;
using Framework;

public class Unit : MonoBehaviour
{
    public event Action<Unit> Moved;
    public event Action<Unit> Died;

    public HealthComponent Health { get; private set; }
    public int MovementRange { get { return _unitData.MovementRange; } }

    [SerializeField] private UnitData _unitData;

    public void Initialize(UnitData unitData)
    {
        _unitData = unitData;

        Health = gameObject.GetOrAddComponent<HealthComponent>();
        Health.Initialize(unitData.MaxHealth);
        Health.Died += HealthComp_Died;
    }

    public void Move(Tile tile)
    {
        if (tile.Occupied)
        {
            Debug.LogWarningFormat("Unit {0} cannot move to tile {1} as it is occupied.", name, tile.name);
            return;
        }

        transform.position = tile.transform.position;
        Moved.InvokeSafe(this);
        tile.SetOccupant(this);
    }

    private void HealthComp_Died(HealthComponent healthComponent)
    {
        Died.InvokeSafe(this);
    }
}
