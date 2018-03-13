using NodeSystem.Editor;

namespace NodeSystem
{
    public class NodeExecutionGroup
    {
        public Node Node { get; private set; }
        public NodeConnection CurrentConnection { get; private set; }
        public NodeExecutionGroup SubGroup { get; private set; }
        public int Depth { get; private set; }
        public bool Finished { get; private set; }

        private NodeGraphHelper _graphHelper;
        private INodeEditorLogger _logger;
        private int _currentPin;
        private int _pinCount;

        public NodeExecutionGroup(INodeEditorLogger logger, int depth, Node node, NodeGraphHelper graphHelper)
        {
            Depth = depth;
            Node = node;

            _logger = logger;
            _graphHelper = graphHelper;
            _pinCount = node.InputPins.Count;
        }

        public void Iterate()
        {
            if (_currentPin == _pinCount || _pinCount == 0)
            {
                Log("Finished iterating on node '{0}'.", Node.Name);
                SubGroup = null;
                Finished = true;
                return;
            }

            Log("Iterate on pin '{0}' on '{1}'...", Node.InputPins[_currentPin].Name, Node.Name);

            if (SubGroup != null)
            {
                if (SubGroup.Finished)
                {
                    Log("Subgroup '{0}' has finished. Setting pin value for '{1}' on '{2}'...",
                        SubGroup.Node.Name,
                        GetExecutionStartNode(CurrentConnection).InputPins[_currentPin].Name,
                        GetExecutionStartNode(CurrentConnection).Name);
                    GetExecutionStartPin(CurrentConnection).SetValueFromPin(GetExecutionEndPin(CurrentConnection));
                    SubGroup = null;
                }
            }
            else
            {
                var pin = Node.InputPins[_currentPin];
                CurrentConnection = _graphHelper.GetConnectionFromEndPin(pin);

                if (CurrentConnection != null)
                {
                    var startPin = GetExecutionStartPin(CurrentConnection);
                    var endPin = GetExecutionEndPin(CurrentConnection);
                    var startNode = GetExecutionStartNode(CurrentConnection);
                    var endNode = GetExecutionEndNode(CurrentConnection);

                    // Early out when hitting an execute node and take the value from it's pin.
                    if (endNode != null && endNode.HasExecutePins())
                    {
                        startPin.SetValueFromPin(endPin);
                    }
                    else
                    {
                        SubGroup = new NodeExecutionGroup(_logger, Depth + 1, GetExecutionEndNode(CurrentConnection), _graphHelper);
                    }
                }
                else
                {
                    SubGroup = null;
                }
            }

            // Not waiting for anything, so iterate pin.
            if (SubGroup == null)
                _currentPin++;
        }

        void Log(string message, params object[] args)
        {
            var prefix = string.Format("Depth: {0} - ", Depth);
            _logger.Log<NodeExecutionGroup>(prefix + message, args);
        }

        NodePin GetExecutionStartPin(NodeConnection connection)
        {
            // Pins flow left to right when of execute type and right to left when of value type.
            return connection.Type == NodeConnectionType.Execute ? connection.SourcePin : connection.TargetPin;
        }

        Node GetExecutionStartNode(NodeConnection connection)
        {
            return connection.Type == NodeConnectionType.Execute ? connection.LeftNode : connection.RightNode;
        }

        NodePin GetExecutionEndPin(NodeConnection connection)
        {
            return connection.Type == NodeConnectionType.Execute ? connection.TargetPin : connection.SourcePin;
        }

        Node GetExecutionEndNode(NodeConnection connection)
        {
            return connection.Type == NodeConnectionType.Execute ? connection.RightNode : connection.LeftNode;
        }
    }
}
