using System;
using System.Collections.Generic;
using NodeSystem.Editor;

namespace NodeSystem
{
    public class NodeGraphRunner
    {
        private const int MaxExecutions = -1;

        private INodeEditorLogger _logger;
        private NodeGraph _graph;
        private NodeRunner _runner;
        private Node _currentNode;
        private Dictionary<string, NodeGraphEvent> _graphEventCache;
        private List<Action<Node>> _callbackRegister;
        private int _executions;

        public NodeGraphRunner(NodeGraph graph)
        {
            if (graph == null)
            {
                _logger.LogError<NodeGraphRunner>("Cannot run graph as it is null.");
                return;
            }

            _graph = graph;

            _logger = NodeEditor.GetNewLoggerInstance();
            _logger.LogLevel = NodeEditorLogLevel.ErrorsAndWarnings;

            _graphEventCache = new Dictionary<string, NodeGraphEvent>();
            _callbackRegister = new List<Action<Node>>();

            _runner = new NodeRunner(_graph.Helper, true);
        }

        public void ExecuteEvent(NodeGraph graph, string eventName)
        {
            NodeGraphEvent startNode = null;

            if (_graphEventCache.ContainsKey(eventName))
            {
                startNode = _graphEventCache[eventName];
            }
            else
            {
                var eventNodes = graph.Helper.GetNodes<NodeGraphEvent>(eventName);

                if (eventNodes.Count == 0)
                {
                    _logger.LogWarning<NodeGraphRunner>("Cannot run graph as no node was found for event '{0}'.", eventName);
                    return;
                }

                if (eventNodes.Count > 1)
                    _logger.LogWarning<NodeGraphRunner>("Found multiple nodes for event '{0}'. Using the first found node...", eventName);

                startNode = eventNodes[0];
            }

            _logger.Log<NodeGraphRunner>("Executing...");

            _currentNode = startNode;
            MoveNext();
        }

        public void RegisterCallback(Action<Node> callback)
        {
            if (_callbackRegister.Contains(callback))
            {
                _logger.LogWarning<NodeGraphRunner>("Cannot add callback as it is already registered.");
            }
            else
            {
                _callbackRegister.Add(callback as Action<Node>);
            }
        }

        public void RemoveCallback(Action<Node> callback)
        {
            if (!_callbackRegister.Contains(callback))
            {
                _logger.LogWarning<NodeGraphRunner>("Cannot remove callback as it was never registered.");
            }
            else
            {
                _callbackRegister.Remove(callback as Action<Node>);
            }
        }

        void MoveNext()
        {
            NodeEditor.Assertions.IsNotNull(_currentNode);

            _logger.Log<NodeGraphRunner>("Move next: {0} ({1})", _currentNode.Name, _currentNode.ID);

            // Run through all the nodes connected to the current node to prepare it for execution.
            _runner.StartFrom(_currentNode, (iteratedNode) => _callbackRegister.ForEach(x => x.Invoke(iteratedNode)));

            var executeHandler = _currentNode as INodeExecuteHandler;
            if (executeHandler != null)
                executeHandler.Execute();

            _executions++;

            if (MaxExecutions != -1 && _executions == MaxExecutions)
            {
                _logger.LogWarning<NodeGraphRunner>("Max executions have been reached!");
                return;
            }
            else
            {
                var executeOutput = _currentNode as INodeExecuteOutput;

                if (executeOutput != null)
                {
                    var connection = _graph.Helper.GetConnectionFromStartPin(executeOutput.ExecuteOut);
                    if (connection != null)
                    {
                        _currentNode = connection.RightNode;
                        MoveNext();
                    }
                }

                _logger.Log<NodeGraphRunner>("Finished execution of all nodes.");
            }
        }
    }
}
