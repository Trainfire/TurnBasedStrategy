using System;
using System.Collections.Generic;
using NodeSystem.Editor;

namespace NodeSystem
{
    public class NodeRunner
    {
        public Node StartNode { get; private set; }
        public bool Completed { get; private set; }

        private NodeGraphHelper _graphHelper;
        private INodeEditorLogger _logger;
        private bool _autoIterate;

        private Stack<NodeExecutionGroup> _stack;
        private Action<Node> _callback;

        public NodeRunner(NodeGraphHelper graphHelper, bool autoIterate = false)
        {
            _graphHelper = graphHelper;
            _autoIterate = autoIterate;

            _logger = NodeEditor.GetNewLoggerInstance();
            _logger.LogLevel = NodeEditorLogLevel.ErrorsAndWarnings;
        }

        public void StartFrom(Node startNode, Action<Node> callback = null)
        {
            StartNode = startNode;
            _callback = callback;

            _stack = new Stack<NodeExecutionGroup>();
            _stack.Push(new NodeExecutionGroup(_logger, 0, startNode, _graphHelper)); // Auto-iterate on start node.

            if (_autoIterate)
                Iterate();
        }

        public void Iterate()
        {
            if (_stack.Count == 0)
            {
                Completed = true;
                return;
            }

            var currentGroup = _stack.Peek();
            _callback.InvokeSafe(currentGroup.Node);
            currentGroup.Iterate();

            if (currentGroup.SubGroup != null)
            {
                _stack.Push(currentGroup.SubGroup);
            }
            else if (currentGroup.Finished)
            {
                currentGroup.Node.Calculate(); // Node is prepared. Calculate it.
                _stack.Pop();
            }

            if (_autoIterate)
                Iterate();
        }
    }
}