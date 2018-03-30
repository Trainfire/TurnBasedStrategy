using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Mech : Unit
{
    public int MovementRange { get { return MechData != null ? MechData.MovementRange : 0; } }

    public MechData MechData { get; private set; }
    public UnitPushableComponent Pushable { get; private set; }
    public UnitWeaponComponent PrimaryWeapon { get; private set; }
    public UnitWeaponComponent SecondaryWeapon { get; private set; }

    public void Initialize(MechData mechData, GameboardHelper helper)
    {
        base.Initialize(helper);

        MechData = mechData;
        SetName(MechData.Name);

        Health.Setup(MechData.MaxHealth);

        Pushable = AddUnitComponent<UnitPushableComponent>();

        PrimaryWeapon = AddUnitComponent<UnitWeaponComponent>();
        PrimaryWeapon.WeaponData = MechData.DefaultPrimaryWeapon;

        SecondaryWeapon = AddUnitComponent<UnitWeaponComponent>();
        SecondaryWeapon.WeaponData = MechData.DefaultSecondaryWeapon;

        Assert.IsNotNull(MechData.View, "Missing view.");
        Instantiate(MechData.View).transform.SetParent(transform);
    }
}
