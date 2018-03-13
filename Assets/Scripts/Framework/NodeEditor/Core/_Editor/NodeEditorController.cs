using NodeSystem;
using System;

namespace NodeSystem.Editor
{
    public class NodeEditorController
    {
        public event Action<NodeGraphData> GraphSaved;

        private NodeGraphRunner _runner;
        private NodeGraph _graph;
        private INodeEditorUserEventsListener _eventListener;

        public NodeEditorController(NodeGraph graph, INodeEditorUserEventsListener inputHandler)
        {
            _graph = graph;

            _eventListener = inputHandler;
            _eventListener.Duplicate += Input_Duplicate;
            _eventListener.Delete += Input_Delete;
            _eventListener.SelectNode += Input_SelectNode;
            _eventListener.SaveGraph += Save;
            _eventListener.RevertGraph += RevertGraph;
            _eventListener.RunGraph += RunGraph;

            _eventListener.AddGraphVariable += Event_AddGraphVariable;
            _eventListener.RemoveGraphVariable += Event_RemoveGraphVaraible;

            _eventListener.AddNode += Event_AddNode;
            _eventListener.AddVariableNode += Event_AddVariableNode;

            //_runner = new NodeGraphRunner();
        }

        public void Load(NodeGraphData graphData)
        {
            NodeEditor.Logger.Log<NodeEditorController>("Loading graph from root...");

            // Copy from existing graph data.
            var editingGraphData = new NodeGraphData(graphData);
            _graph.Load(editingGraphData);
        }

        void Save()
        {
            NodeEditor.Assertions.IsNotNull(_graph, "Graph is null.");

            if (_graph != null)
            {
                var saveData = NodeGraphHelper.GetGraphData(_graph);

                GraphSaved.InvokeSafe(saveData);

                _graph.Load(saveData);
            }
        }

        void RunGraph()
        {
            //_runner.ExecuteEvent(_graph, "Start");
            NodeEditor.Logger.LogWarning<NodeEditorController>("RunGraph is no longer a valid function.");
        }

        void RevertGraph()
        {
            _graph.State.RevertToLastGraph();
        }

        public void ClearGraph()
        {
            _graph.Unload();
        }

        #region Event Callbacks
        void Input_SelectNode(Node node)
        {
            _graph.SetSelection(node);
        }

        void Input_Duplicate()
        {
            if (_graph.Selection != null)
                _graph.Duplicate(_graph.Selection);
        }

        void Input_Delete()
        {
            _graph.RemoveNode(_graph.Selection);
        }

        void Input_RemoveAllNodes()
        {
            if (_graph != null)
                _graph.Unload();
        }

        void Event_AddNode(AddNodeEvent addNodeEvent)
        {
            if (_graph != null)
                _graph.AddNode(addNodeEvent.NodeRegistryEntry);
        }

        void Event_AddVariableNode(AddNodeVariableArgs addNodeVariableArgs)
        {
            _graph.AddNodeVariable(addNodeVariableArgs);
        }

        void Event_RemoveGraphVaraible(RemoveGraphVariableEvent removeGraphVariableEvent)
        {
            _graph.RemoveVariable(removeGraphVariableEvent.Variable);
        }

        void Event_AddGraphVariable(AddGraphVariableEvent addGraphVariableEvent)
        {
            _graph.AddVariable(addGraphVariableEvent.VariableName, addGraphVariableEvent.VariableType);
        }
        #endregion

        public void Destroy()
        {
            _eventListener.Delete -= Input_Delete;
            _eventListener.Duplicate -= Input_Duplicate;
            _eventListener.SelectNode -= Input_SelectNode;
            _eventListener.AddNode -= Event_AddNode;
            _eventListener.RemoveAllNodes -= Input_RemoveAllNodes;
            _eventListener.SaveGraph -= Save;
            _eventListener.RevertGraph -= RevertGraph;
            _eventListener.RunGraph -= RunGraph;

            ClearGraph();
        }
    }

    static class Extensions
    {
        public static void InvokeSafe(this Action action)
        {
            if (action != null)
                action.Invoke();
        }

        public static void InvokeSafe<T>(this Action<T> action, T arg)
        {
            if (action != null)
                action.Invoke(arg);
        }

        public static void InvokeSafe<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            if (action != null)
                action.Invoke(arg1, arg2);
        }
    }

    public interface INodeEditorUserEventsListener
    {
        event Action<Node> SelectNode;
        event Action<NodePin> MouseDownOverPin;
        event Action<NodePin> MouseUpOverPin;
        event Action MouseUp;
        event Action MouseDown;
        event Action<NodePin> MouseHoverEnterPin;
        event Action MouseHoverLeavePin;

        event Action RunGraph;
        event Action SaveGraph;
        event Action RevertGraph;

        event Action<AddNodeEvent> AddNode;
        event Action<AddNodeVariableArgs> AddVariableNode;
        event Action<AddGraphVariableEvent> AddGraphVariable;
        event Action<RemoveGraphVariableEvent> RemoveGraphVariable;
        event Action RemoveAllNodes;
        event Action Duplicate;
        event Action Delete;
    }

    public enum NodeEditorLogLevel
    {
        All,
        ErrorsOnly,
        ErrorsAndWarnings,
    }

    public interface INodeEditorLogger
    {
        NodeEditorLogLevel LogLevel { set; }
        void Log<TSource>(string message, params object[] args);
        void LogWarning<TSource>(string message, params object[] args);
        void LogError<TSource>(string message, params object[] args);
    }

    public interface INodeEditorAssertions
    {
        void WarnIsFalse(bool condition, string message = "");
        void WarnIsTrue(bool condition, string message = "");
        void WarnIsNull<TObject>(TObject value, string message = "") where TObject : class;
        void WarnIsNotNull<TObject>(TObject value, string message = "") where TObject : class;

        void IsFalse(bool condition, string message = "");
        void IsTrue(bool condition, string message = "");
        void IsNull<TObject>(TObject value, string message = "") where TObject : class;
        void IsNotNull<TObject>(TObject value, string message = "") where TObject : class;
    }

    public class NullNodeEditorAssertions : INodeEditorAssertions
    {
        public void WarnIsFalse(bool condition, string message) { }
        public void WarnIsNotNull<TObject>(TObject value, string message) where TObject : class { }
        public void WarnIsNull<TObject>(TObject value, string message) where TObject : class { }
        public void WarnIsTrue(bool condition, string message) { }

        public void IsFalse(bool condition, string message) { }
        public void IsNotNull<TObject>(TObject value, string message) where TObject : class { }
        public void IsNull<TObject>(TObject value, string message) where TObject : class { }
        public void IsTrue(bool condition, string message) { }
    }

    public class NullNodeEditorLogger : INodeEditorLogger
    {
        public NodeEditorLogLevel LogLevel { set { } }
        public void Log<TSource>(string message, params object[] args) { }
        public void LogError<TSource>(string message, params object[] args) { }
        public void LogWarning<TSource>(string message, params object[] args) { }
    }

#region Events
    public class AddNodeEvent
    {
        public NodeRegistryEntry NodeRegistryEntry { get; private set; }

        public AddNodeEvent(NodeRegistryEntry nodeRegistryEntry)
        {
            NodeRegistryEntry = nodeRegistryEntry;
        }
    }

    public class AddNodeVariableArgs
    {
        public NodeGraphVariable Variable { get; private set; }
        public NodeGraphVariableAccessorType AccessorType { get; private set; }

        public AddNodeVariableArgs(NodeGraphVariable variable, NodeGraphVariableAccessorType accessorType)
        {
            Variable = variable;
            AccessorType = accessorType;
        }
    }

    public class AddGraphVariableEvent
    {
        public string VariableName { get; private set; }
        public Type VariableType { get; private set; }

        public AddGraphVariableEvent(Type variableType)
        {
            VariableName = "New Variable";
            VariableType = variableType;
        }

        public AddGraphVariableEvent(string variableName, Type variableType)
        {
            VariableName = variableName;
            VariableType = variableType;
        }
    }

    public class RemoveGraphVariableEvent
    {
        public NodeGraphVariable Variable { get; private set; }

        public RemoveGraphVariableEvent(NodeGraphVariable variable)
        {
            Variable = variable;
        }
    }
    #endregion
}