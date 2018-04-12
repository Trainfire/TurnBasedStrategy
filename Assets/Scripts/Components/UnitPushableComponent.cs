using UnityEngine.Assertions;

public class UnitPushableComponent : UnitComponent
{
    /// <summary>
    /// TODO: Define in global data.
    /// </summary>
    public static readonly int DamageOnCollision = -1;

    public void Push(WorldDirection worldDirection)
    {
        var collisionResults = Helper.GetCollisions(Unit, worldDirection);

        // Push unit if no other units will be hit.
        // Otherwise apply 1 damage to each unit.
        if (collisionResults.Count == 0)
        {
            Unit.MoveInDirection(worldDirection);
        }
        else
        {
            foreach (var pushbackResult in collisionResults)
            {
                Assert.IsNotNull(pushbackResult.Occupant);

                pushbackResult.Occupant.Health.Modify(-1);

                if (Helper.CanReachTile(pushbackResult.transform.GetGridPosition(), worldDirection))
                    Unit.MoveInDirection(worldDirection);
            }
        }
    }
}
