using UnityEngine;
using UnityEngine.Assertions;
using System;
using Framework;

public class Weapon
{
    public WeaponData Data { get; private set; }

    private Unit _owner;
    private GameboardHelper _gameboardHelper;

    public Weapon(Unit owner, WeaponData weaponData, GameboardHelper gameboardHelper)
    {
        _owner = owner;
        Data = weaponData;
        _gameboardHelper = gameboardHelper;
    }

    public void Use(Tile targetTile, Action<bool> onCompleteCallback = null)
    {
        Assert.IsNotNull(_owner, "Owner is null.");
        Assert.IsNotNull(Data, "Weapon data is missing.");
        Assert.IsNotNull(Data.EffectPrototype, "Effect prototype is missing from weapon data.");
        Assert.IsFalse(Data.WeaponType == WeaponType.Invalid);

        if (!_gameboardHelper.CanAttackTile(_owner, targetTile, Data))
        {
            onCompleteCallback.InvokeSafe(false);
            return;
        }

        if (Data.WeaponType == WeaponType.Precision)
        {
            SpawnEffect(targetTile);
            onCompleteCallback.InvokeSafe(true);
        }
        else if (Data.WeaponType == WeaponType.Projectile)
        {
            Assert.IsNotNull(Data.ProjectilePrototype, "Projectile prototype is missing from weapon data.");

            var direction = (targetTile.Position - _owner.Position).normalized;

            var projectile = GameObject.Instantiate<Projectile>(Data.ProjectilePrototype);
            projectile.transform.position = _owner.transform.position;

            projectile.ApplyForce(GridHelper.VectorToDirection(direction), (collision) =>
            {
                var tile = collision.Collider.GetComponentInParent<Tile>();
                if (tile != null && tile.Occupied && tile.Occupant != _owner)
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
        var effectInstance = GameObject.Instantiate<EffectRoot>(Data.EffectPrototype);
        effectInstance.Apply(_gameboardHelper, new UnitAttackEvent(_owner, target, Data));
    }
}