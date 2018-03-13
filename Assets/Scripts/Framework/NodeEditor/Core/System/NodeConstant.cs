using System;
using NodeSystem.Editor;

namespace NodeSystem
{
    public class NodeConstant : Node
    {
        public NodeValueWrapper ValueWrapper { get; private set; }

        protected NodePin Out { get; private set; }

        NodePinValueWrapper _wrappedOutPin;

        protected override void OnInitialize()
        {
            SetType(typeof(NodePinTypeNone));
        }

        public void Set(NodeConstantData nodeConstantData)
        {
            var constantType = Type.GetType(nodeConstantData.ConstantType);
            SetType(Type.GetType(nodeConstantData.ConstantType));
            ValueWrapper.SetFromString(nodeConstantData.Value);
        }

        public void SetType(Type type)
        {
            if (ValueWrapper != null && type == ValueWrapper.ValueType || type == null)
                return;

            if (ValueWrapper != null)
                ValueWrapper.Changed -= ValueWrapper_Changed;

            ValueWrapper = Activator.CreateInstance(typeof(NodeValueWrapper<>).MakeGenericType(type)) as NodeValueWrapper;
            ValueWrapper.Changed += ValueWrapper_Changed;

            UpdateOutPin();
        }

        void UpdateOutPin()
        {
            RemoveAllPins();
            Out = AddPin("Out", ValueWrapper.ValueType, true);
            _wrappedOutPin = NodePinValueWrapper.Instantiate(Out, ValueWrapper);
        }

        void ValueWrapper_Changed(NodeValueWrapper valueWrapper)
        {
            TriggerChange();
        }

        public override void Calculate()
        {
            _wrappedOutPin.Calculate();
        }
    }
}
