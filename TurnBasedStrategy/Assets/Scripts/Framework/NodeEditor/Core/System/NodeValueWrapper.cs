using NodeSystem.Editor;
using System;

namespace NodeSystem
{
    public abstract class NodeValueWrapper
    {
        public event Action<NodeValueWrapper> Changed;

        public abstract Type ValueType { get; }

        public NodeValueWrapper() { }

        public NodeValueWrapper(string value)
        {
            SetFromString(value);
        }

        public abstract void SetFromString(string value);

        protected void TriggerChange()
        {
            NodeEditor.Logger.Log<NodeValueWrapper>("Value changed");
            Changed.InvokeSafe(this);
        }
    }

    public class NodeValueWrapper<T> : NodeValueWrapper
    {
        public override Type ValueType { get { return typeof(T); } }

        protected T _value;
        public T Value
        {
            get { return _value; }
            set
            {
                bool triggerChange = _value != null && !_value.Equals(value);

                _value = value;

                if (triggerChange)
                    TriggerChange();
            }
        }

        public NodeValueWrapper()
        {
            if (typeof(T) == typeof(string))
            {
                (this as NodeValueWrapper<string>).Value = string.Empty;
            }
            else
            {
                Value = default(T);
            }
        }

        public NodeValueWrapper(string value) : base(value)
        {
            SetFromString(value);
        }

        /// <summary>
        /// Sets the value by parsing the string.
        /// </summary>
        public override void SetFromString(string value)
        {
            SetIfType(typeof(float), () =>
            {
                float outValue;
                float.TryParse(value, out outValue);
                Set(outValue);
            });

            SetIfType(typeof(int), () =>
            {
                int outValue;
                int.TryParse(value, out outValue);
                Set(outValue);
            });

            SetIfType(typeof(bool), () =>
            {
                bool outValue;
                bool.TryParse(value, out outValue);
                Set(outValue);
            });

            SetIfType(typeof(string), () => Set(value));
        }

        void SetIfType(Type type, Action onIsType)
        {
            if (typeof(T) == type)
            {
                onIsType();
                TriggerChange();
            }
        }

        public void Set(float value) { (this as NodeValueWrapper<float>).Value = value; }
        public void Set(int value) { (this as NodeValueWrapper<int>).Value = value; }
        public void Set(bool value) { (this as NodeValueWrapper<bool>).Value = value; }
        public void Set(string value) { (this as NodeValueWrapper<string>).Value = value; }

        public override string ToString()
        {
            return Value != null ? Value.ToString() : "None";
        }
    }
}
