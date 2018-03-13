using System;
using NodeSystem.Editor;

namespace NodeSystem
{
    public class NodeGraphState
    {
        public event Action<NodeGraphState> Changed;

        public bool IsDirty { get; private set; }
        public bool GraphLoaded { get; private set; }

        private NodeGraph _graph;
        private NodeGraphData _lastGraphData;

        /// <summary>
        /// Returns a copy of the last graph data.
        /// </summary>
        public NodeGraphData LastGraphData { get { return new NodeGraphData(_lastGraphData); } }

        public NodeGraphState(NodeGraph graph)
        {
            _graph = graph;

            _graph.PostLoad += Graph_PostLoad;
            _graph.PreUnload += Graph_PreUnload;
            _graph.Edited += Graph_Edited;
        }

        public void RevertToLastGraph()
        {
            _graph.Load(_lastGraphData);
        }

        public void Destroy()
        {
            _graph.PostLoad -= Graph_PostLoad;
            _graph.PreUnload -= Graph_PreUnload;
            _graph.Edited -= Graph_Edited;
        }

        void Graph_PostLoad(NodeGraph graph, NodeGraphData graphData)
        {
            GraphLoaded = true;
            _lastGraphData = graphData;
            NodeEditor.Logger.Log<NodeGraphState>("Graph is now loaded. Will now listen for state changes...");
            Changed.InvokeSafe(this);
        }

        void Graph_PreUnload(NodeGraph graph)
        {
            IsDirty = false;
            GraphLoaded = false;
            NodeEditor.Logger.Log<NodeGraphState>("Graph is unloading. No longer listening to state changes...");
            Changed.InvokeSafe(this);
        }

        void Graph_Saved(NodeGraph graph)
        {
            NodeEditor.Logger.Log<NodeGraphState>("Graph was saved.");
            IsDirty = false;
            Changed.InvokeSafe(this);
        }

        void Graph_Edited(NodeGraph graph)
        {
            if (GraphLoaded)
            {
                NodeEditor.Logger.Log<NodeGraphState>("Graph state changed");
                IsDirty = true;
                Changed.InvokeSafe(this);

                // TODO: Record undo changes here?
            }
        }
    }
}