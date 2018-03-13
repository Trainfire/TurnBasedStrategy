using System;
using NodeSystem.Editor;

namespace NodeSystem
{
    public class NodeGraphEvent : Node1Out<NodePinTypeExecute>, INodeExecuteOutput
    {
        public NodePin<NodePinTypeExecute> ExecuteOut { get { return Out; } }
    }

    public class CoreDebugLog : NodeExecute1In1Out<string, string>
    {
        public override void Execute()
        {
            if (In.Value != null)
            {
                NodeEditor.Logger.Log<CoreDebugLog>(In.Value);
            }
            else
            {
                NodeEditor.Logger.Log<CoreDebugLog>("Value is null.");
            }

            Out.Value = In.Value;
        }
    }

    public abstract class MathBase : Node
    {
        private NodePin _in1;
        private NodePin _in2;
        private NodePin _out;
        private Action _onCalculate;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _in1 = AddInputPin<NodePinTypeAny>("In 1");
            _in2 = AddInputPin<NodePinTypeAny>("In 2");
            _out = AddOutputPin<NodePinTypeAny>("Out");
        }

        protected override void OnPinConnected(NodePinConnectEvent pinConnectEvent)
        {
            if (_in1.WrappedType != typeof(NodePinTypeAny) || _in2.WrappedType != typeof(NodePinTypeAny))
                return;

            if (pinConnectEvent.OtherPin.WrappedType == typeof(float))
            {
                _in1 = ChangePinType<float>(_in1);
                _in2 = ChangePinType<float>(_in2);
                _out = ChangePinType<float>(_out);
                _onCalculate = CalculateFloat;
            }
            else if (pinConnectEvent.OtherPin.WrappedType == typeof(int))
            {
                _in1 = ChangePinType<int>(_in1);
                _in2 = ChangePinType<int>(_in2);
                _out = ChangePinType<int>(_out);
                _onCalculate = CalculateInt;
            }
        }

        public override void Calculate() { _onCalculate(); }

        protected void CalculateFloat() { Write(_out, GetFloat(Read<float>(_in1), Read<float>(_in2))); }
        protected void CalculateInt() { Write(_out, GetInt(Read<int>(_in1), Read<int>(_in2))); }

        protected abstract float GetFloat(float a, float b);
        protected abstract int GetInt(int a, int b);
    }

    public class MathAdd : MathBase
    {
        protected override float GetFloat(float a, float b) { return a + b; }
        protected override int GetInt(int a, int b) { return a + b; }
    }

    public class MathSubtract : MathBase
    {
        protected override float GetFloat(float a, float b) { return a - b; }
        protected override int GetInt(int a, int b) { return a - b; }
    }

    public class MathMultiply : MathBase
    {
        protected override float GetFloat(float a, float b) { return a * b; }
        protected override int GetInt(int a, int b) { return a * b; }
    }

    public class MathDivide : MathBase
    {
        protected override float GetFloat(float a, float b) { return a / b; }
        protected override int GetInt(int a, int b) { return a / b; }
    }

    public class ConversionToString<TIn> : Node1In1Out<TIn, string>
    {
        public override void Calculate() { Out.Value = In.ToString(); }
    }

    public class LogicSelect : Node3In1Out<bool, string, string, string>
    {
        public override void Calculate()
        {
            bool condition = In1.Value;
            Out.Value = condition ? In2.Value : In3.Value;
        }
    }

    public class LogicEquals : Node2In1Out<int, int, bool>
    {
        public override void Calculate() { Out.Value = In1.Value == In2.Value; }
    }

    public class LogicNot : Node1In1Out<bool, bool>
    {
        public override void Calculate() { Out.Value = !In.Value; }
    }

    public class LogicAnd : Node2In1Out<bool, bool, bool>
    {
        public override void Calculate() { Out.Value = In1.Value && In2.Value; }
    }

    public class LogicOr : Node2In1Out<bool, bool, bool>
    {
        public override void Calculate() { Out.Value = In1.Value || In2.Value; }
    }

    public class ExecuteBranch : Node1In<bool>, INodeExecuteOutput
    {
        public NodePin<NodePinTypeExecute> ExecuteOut
        {
            get { return In.Value ? _onTrue : _onFalse; }
        }

        private NodePin<NodePinTypeExecute> _onTrue;
        private NodePin<NodePinTypeExecute> _onFalse;

        protected override void OnInitialize()
        {
            AddExecuteInPin();
            base.OnInitialize(); // Add parent input.
            _onTrue = AddExecuteOutPin("True");
            _onFalse = AddExecuteOutPin("False");
        }
    }
}