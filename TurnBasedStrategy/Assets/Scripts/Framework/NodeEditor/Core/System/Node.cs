using System;
using System.Collections.Generic;
using System.Linq;
using NodeSystem.Editor;

namespace NodeSystem
{
    public class NodePinChangedEvent
    {
        public NodePin OldPin { get; private set; }
        public NodePin NewPin { get; private set; }

        public NodePinChangedEvent(NodePin oldPin, NodePin newPin)
        {
            OldPin = oldPin;
            NewPin = newPin;
        }
    }

    [Serializable]
    public struct NodeVec2
    {
        public float x;
        public float y;

        public NodeVec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static NodeVec2 operator +(NodeVec2 v1, NodeVec2 v2)
        {
            return new NodeVec2(v1.x + v2.x, v1.y + v2.y);
        }

        public static NodeVec2 operator -(NodeVec2 v1, NodeVec2 v2)
        {
            return new NodeVec2(v1.x - v2.x, v1.y - v2.y);
        }

        public static NodeVec2 operator *(NodeVec2 v1, NodeVec2 v2)
        {
            return new NodeVec2(v1.x * v2.x, v1.y * v2.y);
        }

        public static NodeVec2 operator /(NodeVec2 v1, NodeVec2 v2)
        {
            return new NodeVec2(v1.x / v2.x, v1.y / v2.y);
        }
    }

    public abstract class Node : IDisposable
    {
        public event Action<Node> Changed;
        public event Action<NodePin> PinAdded;
        public event Action<NodePin> PinRemoved;
        public event Action<NodePinChangedEvent> PinChanged;
        public event Action<Node> Destroyed;

        public List<NodePin> Pins { get; private set; }
        public List<NodePin> InputPins { get; private set; }
        public List<NodePin> OutputPins { get; private set; }

        public virtual string Name { get; private set; }
        public string ID { get; private set; }
        public NodeVec2 Position { get; set; }

        public Node()
        {
            Pins = new List<NodePin>();
            InputPins = new List<NodePin>();
            OutputPins = new List<NodePin>();   
        }

        public void Initialize(NodeData data)
        {
            Name = data.Name;
            ID = data.ID;
            Position = data.Position;

            InputPins.Clear(); // TEMP!
            OutputPins.Clear(); // TEMP!
            OnInitialize();
        }

        protected virtual void OnInitialize() { }

        public virtual void Calculate() { }

        protected NodePin<T> AddInputPin<T>(string name)
        {
            var pin = new NodePin<T>(name, Pins.Count, this);
            RegisterPin(pin);
            InputPins.Add(pin);
            return pin;
        }

        protected NodePin<T> AddOutputPin<T>(string name)
        {
            var pin = new NodePin<T>(name, Pins.Count, this);
            RegisterPin(pin);
            OutputPins.Add(pin);
            return pin;
        }

        protected NodePin AddPin(string name, Type type, bool isOutput)
        {
            var classType = typeof(NodePin<>).MakeGenericType(type);
            var pin = Activator.CreateInstance(classType, name, Pins.Count, this) as NodePin;
            RegisterPin(pin);

            if (isOutput)
            {
                OutputPins.Add(pin);
            }
            else
            {
                InputPins.Add(pin);
            }

            return pin;
        }

        [Obsolete("Use AddInputPin<T> and specify NodePinTypeExecute instead.")]
        protected NodePin<NodePinTypeExecute> AddExecuteInPin()
        {
            var pin = new NodePin<NodePinTypeExecute>("In", Pins.Count, this);
            RegisterPin(pin);
            InputPins.Add(pin);
            return pin;
        }

        [Obsolete("Use AddOutputPin<T> and specify NodePinTypeExecute instead.")]
        protected NodePin<NodePinTypeExecute> AddExecuteOutPin(string name = "Out")
        {
            var pin = new NodePin<NodePinTypeExecute>(name, Pins.Count, this);
            RegisterPin(pin);
            OutputPins.Add(pin);
            return pin;
        }

        protected void RemoveInputPin(int pinIndex)
        {
            if (pinIndex <= InputPins.Count && InputPins.Count > 0)
            {
                NodeEditor.Logger.Log<Node>("Remove input pin.");
                var pin = InputPins[pinIndex];
                RemovePin(pin);
                InputPins.Remove(pin);
            }
        }

        protected void RemoveOutputPin(int pinIndex)
        {
            if (pinIndex <= OutputPins.Count && OutputPins.Count > 0)
            {
                NodeEditor.Logger.Log<Node>("Remove output pin.");
                var pin = OutputPins[pinIndex];
                RemovePin(pin);
                OutputPins.Remove(pin);
            }
        }

        protected void RemoveAllPins()
        {
            Pins.ToList().ForEach(pin => RemovePin(pin));
        }

        public NodePin<T> ChangePinType<T>(NodePin pin)
        {
            NodeEditor.Assertions.IsTrue(Pins.Contains(pin), string.Format("'{0}' does not contains pin '{1}'.", Name, pin.Name));

            if (Pins.Contains(pin))
            {
                var replacementPin = new NodePin<T>(pin.Name, pin.Index, this);
                Pins.Remove(pin);

                // Replace old pin with the new pin at the same index as the old pin.
                var targetList = pin.IsInput ? InputPins : OutputPins;
                targetList.Insert(targetList.IndexOf(pin), replacementPin);
                targetList.Remove(pin);

                Pins.Add(replacementPin);

                NodeEditor.Logger.Log<Node>("Swapped pin '{0}' of type '{1}' for type '{2}'", replacementPin.Name, pin.WrappedType, replacementPin.WrappedType);

                PinChanged.InvokeSafe(new NodePinChangedEvent(pin, replacementPin));

                return replacementPin;
            }

            return null;
        }

        protected void TriggerChange()
        {
            Changed.InvokeSafe(this);
        }

        protected virtual void NameOveride() { }

        protected virtual void OnPinConnected(NodePinConnectEvent pinConnectEvent) { }

        public bool IsInputPin(NodePin pin)
        {
            return InputPins.Contains(pin);
        }

        public bool IsOutputPin(NodePin pin)
        {
            return OutputPins.Contains(pin);
        }

        public bool HasPin(int pinId)
        {
            return pinId < Pins.Count;
        }

        public bool HasExecutePins()
        {
            return Pins.Any(x => x.WrappedType == typeof(NodePinTypeExecute));
        }

        public T Read<T>(NodePin pin)
        {
            return (pin as NodePin<T>).Value;
        }

        public void Write<T>(NodePin pin, T value)
        {
            (pin as NodePin<T>).Value = value;
        }

        /// <summary>
        /// Triggers a change to tell the graph state that this node has moved positions.
        /// </summary>
        public void TriggerPositionChanged()
        {
            TriggerChange();
        }

        void RegisterPin(NodePin pin)
        {
            Pins.Add(pin);
            PinAdded.InvokeSafe(pin);
            pin.Connected += OnPinConnected;
        }

        void RemovePin(NodePin pin)
        {
            Pins.Remove(pin);

            if (pin.IsInput)
            {
                InputPins.Remove(pin);
            }
            else
            {
                OutputPins.Remove(pin);
            }

            PinRemoved.InvokeSafe(pin);
            pin.Connected -= OnPinConnected;
            TriggerChange();
        }

        public void Dispose()
        {
            Changed = null;
            PinAdded = null;
            PinRemoved = null;
            PinChanged = null;
            Destroyed = null;
        }
    }

    public abstract class Node1In<TIn> : Node
    {
        protected NodePin<TIn> In { get; private set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            In = AddInputPin<TIn>("In");
        }
    }

    public abstract class Node1Out<TOut> : Node
    {
        protected NodePin<TOut> Out { get; private set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Out = AddOutputPin<TOut>("Out");
        }
    }

    public abstract class Node1In1Out<TIn, TOut> : Node
    {
        protected NodePin<TIn> In { get; private set; }
        protected NodePin<TOut> Out { get; private set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            In = AddInputPin<TIn>("In");
            Out = AddOutputPin<TOut>("Out");
        }
    }

    public abstract class Node2In1Out<TIn1, TIn2, TOut> : Node
    {
        protected NodePin<TIn1> In1 { get; private set; }
        protected NodePin<TIn2> In2 { get; private set; }
        protected NodePin<TOut> Out { get; private set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            In1 = AddInputPin<TIn1>("In 1");
            In2 = AddInputPin<TIn2>("In 2");
            Out = AddOutputPin<TOut>("Out");
        }
    }

    public abstract class Node3In1Out<TIn1, TIn2, TIn3, TOut> : Node
    {
        protected NodePin<TIn1> In1 { get; private set; }
        protected NodePin<TIn2> In2 { get; private set; }
        protected NodePin<TIn3> In3 { get; private set; }
        protected NodePin<TOut> Out { get; private set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            In1 = AddInputPin<TIn1>("In 1");
            In2 = AddInputPin<TIn2>("In 2");
            In3 = AddInputPin<TIn3>("In 3");
            Out = AddOutputPin<TOut>("Out");
        }
    }

    public abstract class Node1In2Out<TIn, TOut1, TOut2> : Node
    {
        protected NodePin<TIn> In { get; private set; }
        protected NodePin<TOut1> Out1 { get; private set; }
        protected NodePin<TOut2> Out2 { get; private set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            In = AddInputPin<TIn>("In");
            Out1 = AddOutputPin<TOut1>("Out 1");
            Out2 = AddOutputPin<TOut2>("Out 2");
        }
    }

    public abstract class Node1In3Out<TIn, TOut1, TOut2, TOut3> : Node
    {
        protected NodePin<TIn> In { get; private set; }
        protected NodePin<TOut1> Out1 { get; private set; }
        protected NodePin<TOut2> Out2 { get; private set; }
        protected NodePin<TOut3> Out3 { get; private set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            In = AddInputPin<TIn>("In");
            Out1 = AddOutputPin<TOut1>("Out 1");
            Out2 = AddOutputPin<TOut2>("Out 2");
            Out3 = AddOutputPin<TOut3>("Out 3");
        }
    }
}