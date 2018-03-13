using System;
using NodeSystem.Editor;

namespace NodeSystem
{
    public enum NodeGraphVariableAccessorType
    {
        Get,
        GetSet,
        Set,
    }

    public class NodeGraphVariableTypeChangeEvent
    {
        public NodeGraphVariable Variable { get; private set; }
        public Type OldType { get; private set; }
        public Type NewType { get; private set; }

        public NodeGraphVariableTypeChangeEvent(NodeGraphVariable variable, Type oldType, Type newType)
        {
            Variable = variable;
            OldType = oldType;
            NewType = newType;
        }
    }

    public class NodeGraphVariableNameChangeRequestEvent
    {
        public NodeGraphVariable Variable { get; private set; }
        public string ReplacementName { get; private set; }

        public NodeGraphVariableNameChangeRequestEvent(NodeGraphVariable variable, string replacementName)
        {
            Variable = variable;
            ReplacementName = replacementName;
        }
    }

    public class NodeGraphVariable
    {
        public event Action<NodeGraphVariableTypeChangeEvent> PreTypeChanged;
        public event Action<NodeGraphVariableTypeChangeEvent> PostTypeChanged;
        public event Action<NodeGraphVariableNameChangeRequestEvent> NameChangeRequested;
        public event Action<NodeGraphVariable> NameChanged;
        public event Action<NodeGraphVariable> Removed;

        string _name;
        /// <summary>
        /// Do not set directly. If you want to set the name, use SetName instead as it needs to be validated by the graph.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NameChanged.InvokeSafe(this);
            }
        }

        /// <summary>
        /// Triggers a request to replace the current name with the specified value.
        /// </summary>
        public string ReplacementName
        {
            set
            {
                if (value != Name)
                    NameChangeRequested.InvokeSafe(new NodeGraphVariableNameChangeRequestEvent(this, value));
            }
        }

        public string ID { get; private set; }

        public Type WrappedType { get { return WrappedValue != null ? WrappedValue.ValueType : null; } }
        public NodeValueWrapper WrappedValue { get; private set; }

        public NodeGraphVariable(NodeGraphVariableData data)
        {
            Name = data.Name;
            ID = data.ID;

            SetValueWrapperFromType(Type.GetType(data.VariableType));
            WrappedValue.SetFromString(data.Value);
        }

        /// <summary>
        /// Sets the wrapped value based on the specified type.
        /// </summary>
        public void SetValueWrapperFromType(Type wrappedValueType)
        {
            if (wrappedValueType == WrappedType)
                return;

            PreTypeChanged.InvokeSafe(new NodeGraphVariableTypeChangeEvent(this, WrappedType, wrappedValueType));

            NodeEditor.Assertions.IsNotNull(wrappedValueType);
            var classType = typeof(NodeValueWrapper<>).MakeGenericType(wrappedValueType);
            WrappedValue = Activator.CreateInstance(classType) as NodeValueWrapper;

            PostTypeChanged.InvokeSafe(new NodeGraphVariableTypeChangeEvent(this, WrappedType, wrappedValueType));
        }

        public void Remove()
        {
            Removed.InvokeSafe(this);
            WrappedValue = null;
        }
    }
}