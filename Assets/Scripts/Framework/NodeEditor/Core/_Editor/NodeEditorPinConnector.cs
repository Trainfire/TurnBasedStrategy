namespace NodeSystem.Editor
{
    public interface INodeEditorPinConnectorView
    {
        string Tooltip { get; set; }
        NodePin EndPin { set; }
        void EnterDrawState(NodePin startPin);
        void EndDrawState();
    }

    public class NodeEditorPinConnector
    {
        private enum ValidationResult
        {
            Valid,
            Invalid,
            ConnectingToSelf,
            IncompatiblePins,
        }

        private NodeGraph _graph;
        private INodeEditorPinConnectorView _view;
        private INodeEditorUserEventsListener _input;

        private NodeConnection _modifyingConnection;
        private NodePin _sourcePin;
        private NodePin _targetPin;

        public NodeEditorPinConnector(NodeGraph graph, INodeEditorPinConnectorView view, INodeEditorUserEventsListener input)
        {
            _graph = graph;

            _view = view;
            _input = input;
            _input.MouseDownOverPin += Input_SelectPin;
            _input.MouseUpOverPin += Input_MouseUpOverPin;
            _input.MouseUp += Input_MouseUp;
            _input.MouseHoverEnterPin += Input_MouseHoverEnterPin;
            _input.MouseHoverLeavePin += Input_MouseHoverLeavePin;
        }

        void Input_MouseHoverLeavePin()
        {
            _view.Tooltip = string.Empty;
        }

        void Input_MouseHoverEnterPin(NodePin nodePin)
        {
            var validationResult = ValidateConnection(nodePin);

            if (validationResult == ValidationResult.Valid)
            {
                _view.Tooltip = "Connect";
            }
            else
            {
                _view.Tooltip = GetErrorMessage(validationResult);
            }

            _view.EndPin = nodePin;
        }

        void Input_SelectPin(NodePin nodePin)
        {
            if (nodePin.IsInput)
            {
                _modifyingConnection = _graph.Helper.GetConnection(nodePin);

                if (_modifyingConnection != null)
                {
                    NodeEditor.Logger.Log<NodeEditorPinConnector>("Modifying a connection...");

                    _modifyingConnection.Hide();

                    _sourcePin = _modifyingConnection.SourcePin;

                    // Executes flow left to right so get the correct starting pin.
                    _view.EnterDrawState(_modifyingConnection.SourcePin);
                }
            }
            else
            {
                NodeEditor.Logger.Log<NodeEditorPinConnector>("Creating a new connection...");

                _sourcePin = nodePin;

                _view.EnterDrawState(nodePin);
            }
        }

        void Input_MouseUp()
        {
            if (_modifyingConnection != null)
            {
                NodeEditor.Logger.Log<NodeEditorPinConnector>("Removing a modified connection.");
                _graph.Disconnect(_modifyingConnection);
            }

            _view.EndDrawState();
            _modifyingConnection = null;
            _sourcePin = null;
        }

        void Input_MouseUpOverPin(NodePin targetPin)
        {
            _targetPin = targetPin;

            if (ValidateConnection(targetPin) != ValidationResult.Valid)
            {
                NodeEditor.Logger.Log<NodeEditorPinConnector>(GetErrorMessage(ValidateConnection(targetPin)));
                return;
            }

            NodeEditor.Logger.Log<NodeEditorPinConnector>("Connecting pin '{0}' in '{1}' to '{2}' in '{3}'",
                _sourcePin.Name,
                _sourcePin.Node.Name,
                targetPin.Name,
                targetPin.Node.Name);

            NodeConnection connection = null;
            NodePin startPin = null;
            NodePin endPin = null;

            if (IsModifyingConnection())
            {
                startPin = _modifyingConnection.SourcePin;   
            }
            else
            {
                startPin = _sourcePin;
            }

            endPin = targetPin;

            connection = new NodeConnection(startPin, endPin);

            if (IsModifyingConnection())
            {
                _graph.Replace(_modifyingConnection, connection);
            }
            else
            {
                _graph.Connect(connection);
            }

            _view.EndDrawState();

            _modifyingConnection = null;
            _sourcePin = null;
        }

        bool IsModifyingConnection()
        {
            return _modifyingConnection != null;
        }

        ValidationResult ValidateConnection(NodePin targetPin)
        {
            if (targetPin == null || _sourcePin == null || targetPin.IsOutput || _sourcePin.IsInput)
                return ValidationResult.Invalid;

            if (!IsModifyingConnection() && targetPin.Node == _sourcePin.Node)
            {
                NodeEditor.Logger.LogWarning<NodeEditorPinConnector>("Attempted to connect a pin to itself!");
                return ValidationResult.ConnectingToSelf;
            }

            if (!targetPin.ArePinsCompatible(_sourcePin))
                return ValidationResult.IncompatiblePins;

            return ValidationResult.Valid;
        }

        string GetErrorMessage(ValidationResult connectionError)
        {
            switch (connectionError)
            {
                case ValidationResult.Invalid: return "Invalid";
                case ValidationResult.ConnectingToSelf: return "Cannot connect to self";
                case ValidationResult.IncompatiblePins: return "Incompatible pins";
                default: return string.Empty;
            }
        }
    }
}
