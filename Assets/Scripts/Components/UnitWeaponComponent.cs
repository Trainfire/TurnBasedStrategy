﻿using UnityEngine;
using UnityEngine.Assertions;
using Framework;
using System;

public class UnitWeaponComponent : UnitComponent
{
    public WeaponData WeaponData { get; set; }

    public void Use(Tile targetTile, Action<bool> onCompleteCallback = null)
    {
        Assert.IsNotNull(Unit, "Owner is null.");
        Assert.IsNotNull(WeaponData, "Weapon data is missing.");
        Assert.IsNotNull(WeaponData.EffectPrototype, "Effect prototype is missing from weapon data.");
        Assert.IsFalse(WeaponData.WeaponType == WeaponType.Invalid);

        if (!Helper.CanAttackTile(Unit, targetTile, WeaponData))
        {
            onCompleteCallback.InvokeSafe(false);
            return;
        }

        if (WeaponData.WeaponType == WeaponType.Precision)
        {
            SpawnEffect(targetTile);
            onCompleteCallback.InvokeSafe(true);
        }
        else if (WeaponData.WeaponType == WeaponType.Projectile)
        {
            Assert.IsNotNull(WeaponData.ProjectilePrototype, "Projectile prototype is missing from weapon data.");

            var direction = (targetTile.transform.GetGridPosition() - Unit.transform.GetGridPosition()).normalized;

            var projectile = GameObject.Instantiate<Projectile>(WeaponData.ProjectilePrototype);
            projectile.transform.position = Unit.transform.position;

            projectile.ApplyForce(GridHelper.VectorToDirection(direction), (collision) =>
            {
                var tile = collision.Collider.GetComponentInParent<Tile>();
                if (tile != null && tile.Occupant != null && tile.Occupant != Unit)
                {
                    SpawnEffect(tile);
                    GameObject.Destroy(projectile.gameObject);
                }
            });
        }
    }

    private void SpawnEffect(Tile target)
    {
        Assert.IsNotNull(target);
        Effect.Spawn(WeaponData.EffectPrototype, (effect) =>
        {
            effect.Apply(Helper, new SpawnEffectParameters(Helper.GetTile(Unit.transform.GetGridPosition()), target));
        });
    }
}
