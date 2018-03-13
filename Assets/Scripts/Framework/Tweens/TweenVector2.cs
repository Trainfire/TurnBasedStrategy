using UnityEngine;

namespace Framework
{
    public class TweenVector2 : Tween<Vector2>
    {
        protected override Vector2 OnTween(float t)
        {
            return Vector2.Lerp(From, To, t);
        }
    }
}
