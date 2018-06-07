using UnityEngine;
using System;
using System.Collections.Generic;
using Framework;

public class AIControllerComponent : UnitComponent
{
    public Tile Target { get; set; }

    private UnitMoveAnimator _moveAnimator;

    protected override void OnInitialize()
    {
        base.OnInitialize();

        _moveAnimator = gameObject.GetOrAddComponent<UnitMoveAnimator>();
    }

    public void Move(Action onMoveComplete)
    {
        if (Target != null)
        {
            var adjacentTiles = new List<Tile>();
            foreach (var tileResult in Helper.GetTilesInRadius(Target, 1))
            {
                if (!tileResult.Tile.Blocked)
                    adjacentTiles.Add(tileResult.Tile);
            }

            if (adjacentTiles.Count == 0)
            {
                DebugEx.LogWarning<StateEnemyThinkPhase>("Failed to find any free tiles surrounding the target '{0}'", Target.name);
            }
            else
            {
                var rnd = UnityEngine.Random.Range(0, adjacentTiles.Count);
                var moveTarget = adjacentTiles[rnd];
                _moveAnimator.Animate(Helper, Unit, moveTarget, () =>
                {
                    Unit.MoveTo(moveTarget);
                    onMoveComplete();
                });
            }
        }
        else
        {
            onMoveComplete();
        }
    }

    public void Act(Action onActComplete)
    {
        if (Target != null)
            Unit.GetComponent<UnitWeaponComponent>()?.Use(Target);

        onActComplete();
    }
}
