using System;
using NodeSystem.Editor;

namespace NodeSystem
{
    public class NodeVariable : Node, INodeExecuteOutput
    {
        public override string Name { get { return _variable != null ? "(V) " + _variable.Name : "(V) N/A"; } }
        public string VariableID { get { return _variable != null ? _variable.ID : string.Empty; } }

        public NodeGraphVariableAccessorType AccessorType { get; private set; }

        protected NodePin In { get; private set; }
        protected NodePin Out { get; private set; }

        NodeGraphVariable _variable;
        NodePinValueWrapper _wrappedIn;
        NodePinValueWrapper _wrappedOut;

        public NodePin<NodePinTypeExecute> ExecuteOut { get; private set; }

        public void Set(NodeGraphVariable variable, NodeGraphVariableAccessorType accessorType)
        {
            AccessorType = accessorType;

            if (variable == null)
            {
                NodeEditor.Logger.LogWarning<NodeVariable>("Variable ({0}) spawned with a null variable. The variable probably doesn't exist.", ID);
            }
            else
            {
                _variable = variable;
                _variable.PreTypeChanged += Variable_PreTypeChanged;
                _variable.PostTypeChanged += Variable_PostTypeChanged;
                _variable.Removed += Variable_Removed;

                SpawnPins();
            }
        }

        void SpawnPins()
        {
            if (AccessorType == NodeGraphVariableAccessorType.GetSet || AccessorType == NodeGraphVariableAccessorType.Set)
            {
                AddInputPin<NodePinTypeExecute>("In");
                ExecuteOut = AddOutputPin<NodePinTypeExecute>("Out");
            }

            if (AccessorType == NodeGraphVariableAccessorType.GetSet || AccessorType == NodeGraphVariableAccessorType.Get)
                AddGetPin();

            if (AccessorType == NodeGraphVariableAccessorType.GetSet || AccessorType == NodeGraphVariableAccessorType.Set)
                AddSetPin();
        }

        void AddGetPin()
        {
            Out = AddPin("Get", _variable.WrappedType, true);
            _wrappedOut = NodePinValueWrapper.Instantiate(Out, _variable.WrappedValue);
        }

        void AddSetPin()
        {
            In = AddPin("Set", _variable.WrappedType, false);
            _wrappedIn = NodePinValueWrapper.Instantiate(In, _variable.WrappedValue);
        }

        void Variable_PreTypeChanged(NodeGraphVariableTypeChangeEvent variableTypeChangeEvent)
        {
            ExecuteOut = null;
            RemoveAllPins();
        }

        void Variable_PostTypeChanged(NodeGraphVariableTypeChangeEvent variableTypeChangeEvent)
        {
            // The type changed so update pins to reflect new type.
            SpawnPins();
        }

        void Variable_Removed(NodeGraphVariable variable)
        {
            _variable = null;
            RemoveAllPins();
        }

        public override void Calculate()
        {
            // Calculate value for Set
            if (In != null)
            {
                NodeEditor.Assertions.IsNotNull(_wrappedIn, "Missing wrapper for Set pin");
                if (_wrappedIn != null)
                    _wrappedIn.Calculate();
            }

            // Calculcate value for Get
            if (Out != null)
            {
                NodeEditor.Assertions.IsNotNull(_wrappedOut, "Missing wrapped for Get pin");
                if (_wrappedOut != null)
                    _wrappedOut.Calculate();
            }
        }
    }
}
