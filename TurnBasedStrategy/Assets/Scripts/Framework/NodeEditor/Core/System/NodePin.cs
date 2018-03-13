using System;
using System.Collections.Generic;
using System.Linq;
using NodeSystem.Editor;

namespace NodeSystem
{
    public enum NodePinTypeCategory
    {
        Uncategorized,
        Special,
        Constant,
    }

    public class NodePinType
    {
        public string Name { get; private set; }
        public Type WrappedType { get; private set; }
        public NodePinTypeCategory Category { get; private set; }
        public List<Type> CompatibleTypes { get; private set; }

        public NodePinType(string name, Type type, NodePinTypeCategory category, params Type[] compatibleTypes)
        {
            Name = name;
            WrappedType = type;
            Category = category;

            CompatibleTypes = new List<Type>();
            CompatibleTypes.Add(type);
            foreach (var compatibleType in compatibleTypes)
            {
                CompatibleTypes.Add(compatibleType);
            }
        }

        public bool AreTypesCompatible<T>()
        {
            return CompatibleTypes.Any(x => x == typeof(T));
        }
    }

    public class NodePinTypeNone { }
    public class NodePinTypeExecute { }
    public class NodePinTypeAny { }

    public class NodePinConnectEvent
    {
        public NodePin Pin { get; private set; }
        public NodePin OtherPin { get; private set; }

        public NodePinConnectEvent(NodePin pin, NodePin otherPin)
        {
            Pin = pin;
            OtherPin = otherPin;
        }
    }

    public abstract class NodePin
    {
        public event Action<NodePinConnectEvent> Connected;
        public event Action<NodePin> Disconnected;

        public Node Node { get; private set; }
        public string Name { get; private set; }
        public int Index { get; private set; }
        public abstract Type WrappedType { get; }
        public bool IsInput { get { return Node.IsInputPin(this); } }
        public bool IsOutput { get { return Node.IsOutputPin(this); } }

        public NodePin(string name, int index, Node node)
        {
            Index = index;
            Name = name;
            Node = node;
        }

        public void Connect(NodePin otherPin)
        {
            Connected.InvokeSafe(new NodePinConnectEvent(this, otherPin));
        }

        public void Disconnect()
        {
            Disconnected.InvokeSafe(this);
            OnDisconnect();
        }

        public bool ArePinsCompatible(NodePin pin)
        {
            var areWrappedTypesCompatible = pin.WrappedType == this.WrappedType || this.WrappedType == typeof(NodePinTypeAny);
            var areSameType = IsSameType(pin);
            var isNoneType = pin.WrappedType == typeof(NodePinTypeNone);
            return areWrappedTypesCompatible && !areSameType && !isNoneType;
        }

        //public bool IsInput()
        //{
        //    return Node.IsInputPin(this);
        //}

        //public bool IsOutput()
        //{
        //    return Node.IsOutputPin(this);
        //}

        /// <summary>
        /// Returns true if both pins are inputs or outputs;
        /// </summary>
        public bool IsSameType(NodePin otherPin)
        {
            return IsInput && otherPin.IsInput || IsOutput && otherPin.IsOutput;
        }

        public bool IsExecutePin()
        {
            return WrappedType == typeof(NodePinTypeExecute);
        }

        public virtual void SetValueFromPin(NodePin pin) { }

        protected virtual void OnDisconnect() { }
    }

    public class NodePin<T> : NodePin
    {
        public NodePin(string name, int index, Node node) : base(name, index, node) { }

        public override Type WrappedType { get { return typeof(T); } }

        private T _value;
        public T Value
        {
            get
            {
                if (_value == null)
                    _value = default(T);

                return _value;
            }
            set
            {
                _value = value;
            }
        }

        public override void SetValueFromPin(NodePin pin)
        {
            NodePin<T> convertedPin = null;
            try
            {
                convertedPin = pin as NodePin<T>;
            }
            catch (Exception ex)
            {
                NodeEditor.Logger.LogError<NodePin>(ex.Message);
            }

            _value = convertedPin.Value;
        }

        protected override void OnDisconnect()
        {
            _value = default(T);
        }

        public override string ToString()
        {
            return _value != null ? _value.ToString() : string.Empty;
        }
    }
}