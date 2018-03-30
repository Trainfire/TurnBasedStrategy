using UnityEngine;
using UnityEngine.Assertions;
using Framework;

public class UnitPushableComponent : UnitComponent
{
    public void Push(WorldDirection worldDirection)
    {
        var pushbackResults = Helper.GetPushbackResults(ParentUnit, worldDirection);

        // Push unit if no other units will be hit.
        // Otherwise apply 1 damage to each unit.
        if (pushbackResults.Count == 1)
        {
            Assert.IsTrue(pushbackResults[0].Unit == ParentUnit);
            ParentUnit.MoveInDirection(worldDirection);
        }
        else
        {
            foreach (var pushbackResult in pushbackResults)
            {
                pushbackResult.Unit.Health.Modify(-1);

                if (Helper.CanReachTile(pushbackResult.Unit.transform.GetGridPosition(), worldDirection))
                    ParentUnit.MoveInDirection(worldDirection);
            }
        }
    }
}
