using UnityEditor;
using NodeSystem;
using NodeSystem.Editor;

namespace Framework
{
    /// <summary>
    /// Finds the root gameobject to load/save graph data to and from.
    /// </summary>
    public class NodeEditorRootFinder
    {
        private NodeGraphRoot _root;
        private NodeEditorController _controller;

        public NodeEditorRootFinder(NodeEditorController controller)
        {
            _controller = controller;
            _controller.GraphSaved += Controller_GraphSaved;

            Selection.selectionChanged += LoadGraphFromSelection;
        }

        void Controller_GraphSaved(NodeGraphData obj)
        {
            _root.GraphData = obj;
        }

        void LoadGraphFromSelection()
        {
            if (Selection.activeGameObject == null)
                return;

            var graphRootFromSelection = Selection.activeGameObject.GetComponentInParent<NodeGraphRoot>();

            bool selectionChanged = graphRootFromSelection == null || graphRootFromSelection != _root;
            if (selectionChanged)
                _controller.ClearGraph();

            // Assign new root.
            _root = graphRootFromSelection;

            if (_root != null)
                _controller.Load(_root.GraphData);
        }

        public void Destroy()
        {
            _controller.GraphSaved -= Controller_GraphSaved;

            Selection.selectionChanged -= LoadGraphFromSelection;
        }
    }
}