using UnityEngine.Assertions;

public class Enemy : Unit
{
    public EnemyData Data { get; private set; }
    public UnitPushableComponent Pushable { get; private set; }
    public UnitWeaponComponent Weapon { get; private set; }
    public AIControllerComponent Controller { get; private set; }

    public void Initialize(EnemyData enemyData, Helper helper)
    {
        base.Initialize(helper);

        Data = enemyData;
        SetName(Data.Name);

        Health.Setup(Data.MaxHealth);
        Health.Killed += OnHealthKilled;

        Pushable = AddUnitComponent<UnitPushableComponent>();

        Weapon = AddUnitComponent<UnitWeaponComponent>();
        Weapon.WeaponData = Data.Weapon;

        Controller = AddUnitComponent<AIControllerComponent>();

        Assert.IsNotNull(Data.View, "Missing view.");
        Instantiate(Data.View).transform.SetParent(transform, false);
    }

    private void OnHealthKilled(HealthComponent healthComponent)
    {
        healthComponent.Killed -= OnHealthKilled;
        RemoveSelf();
    }
}
