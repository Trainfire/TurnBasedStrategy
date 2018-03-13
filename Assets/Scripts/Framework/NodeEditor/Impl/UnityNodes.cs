using UnityEngine;
using NodeSystem;

namespace Framework
{
    public class UnityTimeSinceLevelLoad : Node1Out<float>
    {
        public override void Calculate()
        {
            Write(Out, Time.timeSinceLevelLoad);
        }
    }

    public class UnityMathSplit2 : Node1In2Out<Vector2, float, float>
    {
        public override void Calculate()
        {
            Write(Out1, In.Value.x);
            Write(Out2, In.Value.y);
        }
    }

    public class UnityMathCombine2 : Node2In1Out<float, float, Vector2>
    {
        public override void Calculate()
        {
            Write(Out, new Vector2(In1.Value, In2.Value));
        }
    }

    public class UnityMathSplit3 : Node1In3Out<Vector3, float, float, float>
    {
        public override void Calculate()
        {
            Write(Out1, In.Value.x);
            Write(Out2, In.Value.y);
            Write(Out3, In.Value.z);
        }
    }

    public class UnityMathCombine3 : Node3In1Out<float, float, float, Vector3>
    {
        public override void Calculate()
        {
            Write(Out, new Vector3(In1.Value, In2.Value, In3.Value));
        }
    }

    public class UnityMathSin : Node1In1Out<float, float>
    {
        public override void Calculate()
        {
            Write(Out, Mathf.Sin(In.Value));
        }
    }
}
