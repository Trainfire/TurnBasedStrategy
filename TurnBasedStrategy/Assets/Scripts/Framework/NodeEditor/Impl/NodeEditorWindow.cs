using UnityEngine.Assertions;
using UnityEditor;
using Framework.NodeEditorViews;
using NodeSystem;
using NodeSystem.Editor;
using System;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// Wraps the actual NodeEditor.
    /// </summary>
    public class NodeEditorWindow : EditorWindow
    {
        private const string WindowName = "Node Editor";

        private NodeGraph _graph;
        private NodeEditorView _view;
        private NodeEditorUserEventsListener _input;
        private NodeEditorController _controller;
        private NodeEditorPinConnector _pinConnector;

        private NodeEditorRootFinder _rootHandler;

        public NodeEditorWindow()
        {
            NodeEditor.Logger = new NodeEditorLogger();
            NodeEditor.Assertions = new NodeEditorAssertions();
            NodeEditor.InstantiateLoggerFunc = () => new NodeEditorLogger();

            _graph = new NodeGraph();
            _graph.SetDefaultGraphType(new GameObjectGraphType());
            _view = new NodeEditorView(_graph.Helper);
            _input = new NodeEditorUserEventsListener(_view);
            _controller = new NodeEditorController(_graph, _input);
            _pinConnector = new NodeEditorPinConnector(_graph, _view.ConnectorView, _input);
            _rootHandler = new NodeEditorRootFinder(_controller);
        }

        void OnGUI()
        {
            _input.Update();
            _view.Draw(BeginWindows, EndWindows);
            Repaint();
        }

        [MenuItem("Window/Node Editor")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<NodeEditorWindow>(WindowName);
        }

        void OnDestroy()
        {
            _controller.Destroy();
        }
    }

    public class NodeEditorLogger : INodeEditorLogger
    {
        public NodeEditorLogLevel LogLevel { set { _logLevel = value; } }

        NodeEditorLogLevel _logLevel;

        public NodeEditorLogger(NodeEditorLogLevel logLevel = NodeEditorLogLevel.All)
        {
            _logLevel = logLevel;
        }

        public void Log<TSource>(string message, params object[] args)
        {
            if (_logLevel == NodeEditorLogLevel.All)
                DebugEx.Log<TSource>(message, args);
        }
        public void LogError<TSource>(string message, params object[] args)
        {
            if (_logLevel == NodeEditorLogLevel.All || _logLevel == NodeEditorLogLevel.ErrorsOnly || _logLevel == NodeEditorLogLevel.ErrorsAndWarnings)
                DebugEx.LogError<TSource>(message, args);
        }
        public void LogWarning<TSource>(string message, params object[] args)
        {
            if (_logLevel == NodeEditorLogLevel.All || _logLevel == NodeEditorLogLevel.ErrorsAndWarnings)
             DebugEx.LogWarning<TSource>(message, args);
        }
    }

    public class NodeEditorAssertions : INodeEditorAssertions
    {
        public NodeEditorAssertions(bool throwExceptionsOnAssert = true)
        {
            Assert.raiseExceptions = throwExceptionsOnAssert;
        }

        public void IsFalse(bool condition, string message)
        {
            Assert.IsFalse(condition, message);
        }

        public void IsTrue(bool condition, string message)
        {
            Assert.IsTrue(condition, message);
        }

        public void IsNotNull<TObject>(TObject value, string message) where TObject : class
        {
            Assert.IsNotNull(value, message);
        }

        public void IsNull<TObject>(TObject value, string message) where TObject : class
        {
            Assert.IsNull(value, message);
        }

        public void WarnIsFalse(bool condition, string message = "")
        {
            if (condition)
                Debug.LogWarning(message);
        }

        public void WarnIsTrue(bool condition, string message = "")
        {
            if (!condition)
                Debug.LogWarning(message);
        }

        public void WarnIsNull<TObject>(TObject value, string message = "") where TObject : class
        {
            if (value != null)
                Debug.LogWarning(message);
        }

        public void WarnIsNotNull<TObject>(TObject value, string message = "") where TObject : class
        {
            if (value == null)
                Debug.LogWarning(message);
        }
    }
}
