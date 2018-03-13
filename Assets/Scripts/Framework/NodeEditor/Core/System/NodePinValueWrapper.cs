using System;
using NodeSystem.Editor;

namespace NodeSystem
{
    /// <summary>
    /// Binds a pin to a value wrapper. Used for pins that are bound to a value such as those on NodeVariable and NodeConstant.
    /// </summary>
    public class NodePinValueWrapper
    {
        protected NodePin Pin { get; private set; }
        protected NodeValueWrapper ValueWrapper { get; private set; }

        public NodePinValueWrapper(NodePin pin, NodeValueWrapper valueWrapper)
        {
            Pin = pin;
            ValueWrapper = valueWrapper;
        }

        public virtual void Calculate() { }

        public static NodePinValueWrapper Instantiate(NodePin pin, NodeValueWrapper valueWrapper)
        {
            var classType = typeof(NodePinWrapper<>).MakeGenericType(valueWrapper.ValueType);
            return Activator.CreateInstance(classType, pin, valueWrapper) as NodePinValueWrapper;
        }
    }

    public class NodePinWrapper<T> : NodePinValueWrapper
    {
        public NodePinWrapper(NodePin pin, NodeValueWrapper valueWrapper) : base(pin, valueWrapper) { }

        public override void Calculate()
        {
            NodeEditor.Assertions.IsNotNull(Pin as NodePin<T>);
            NodeEditor.Assertions.IsNotNull(ValueWrapper as NodeValueWrapper<T>);

            if (Pin.IsInput)
            {
                (ValueWrapper as NodeValueWrapper<T>).Value = (Pin as NodePin<T>).Value;
            }
            else
            {
                (Pin as NodePin<T>).Value = (ValueWrapper as NodeValueWrapper<T>).Value;
            }
        }
    }
}
