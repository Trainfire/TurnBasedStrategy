using System;
using System.Collections.Generic;
using System.Linq;
using NodeSystem.Editor;

namespace NodeSystem
{
    /// <summary>
    /// Wrapper to expose graph info safely.
    /// </summary>
    public class NodeGraphHelper : IDisposable
    {
        public event Action GraphPostUnloaded;
        public event Action GraphPostLoaded;
        public event Action<NodeGraphVariable> VariableAdded;
        public event Action<NodeGraphVariable> VariableRemoved;
        public event Action<Node> NodeSelected;
        public event Action<Node> NodeAdded;
        public event Action<Node> NodeRemoved;

        public bool IsGraphDirty { get { return _graph.State.IsDirty; } }
        public bool IsGraphLoaded { get { return _graph.State.GraphLoaded; } }
        public Node SelectedNode { get { return _graph.Selection; } }

        public int NodeCount
        {
            get { return _graph != null ? _graph.Nodes.Count : 0; }
        }

        public List<NodeConnection> Connections
        {
            get { return _graph != null ? _graph.Connections.ToList() : new List<NodeConnection>(); }
        }

        public List<NodeGraphVariable> Variables
        {
            get { return _graph != null ? _graph.Variables.ToList() : new List<NodeGraphVariable>(); }
        }

        public List<NodeRegistryEntry> NodeRegister
        {
            get { return _graph != null ? _graph.GraphType.NodeRegister.ToList() : new List<NodeRegistryEntry>(); }
        }

        public List<NodePinType> PinTypeRegister
        {
            get { return _graph != null ? _graph.GraphType.PinTypeRegister.ToList() : new List<NodePinType>(); }
        }

        public NodeGraphType GraphType
        {
            get { return _graph != null ? _graph.GraphType : null; }
        }

        private NodeGraph _graph;

        public NodeGraphHelper(NodeGraph graph)
        {
            if (graph == null)
                return;

            _graph = graph;
            _graph.PostLoad += Graph_PostLoad;
            _graph.PostUnload += Graph_PostUnload;
            _graph.VariableAdded += Graph_VariableAdded;
            _graph.VariableRemoved += Graph_VariableRemoved;
            _graph.NodeAdded += Graph_NodeAdded;
            _graph.NodeRemoved += Graph_NodeRemoved;
            _graph.NodeSelected += Graph_NodeSelected;
        }

        void Graph_PostUnload(NodeGraph graph) { GraphPostUnloaded.InvokeSafe(); }
        void Graph_PostLoad(NodeGraph graph, NodeGraphData graphData) { GraphPostLoaded.InvokeSafe(); }

        void Graph_VariableRemoved(NodeGraphVariable variable) { VariableRemoved.InvokeSafe(variable); }
        void Graph_VariableAdded(NodeGraphVariable variable) { VariableAdded.InvokeSafe(variable); }

        void Graph_NodeAdded(Node node) { NodeAdded.InvokeSafe(node); }
        void Graph_NodeRemoved(Node node) { NodeRemoved.InvokeSafe(node); }
        void Graph_NodeSelected(Node node) { NodeSelected.InvokeSafe(node); }

        public bool IsPinConnected(NodePin pin)
        {
            return Connections.Any(connection => connection.SourcePin == pin || connection.TargetPin == pin);
        }

        public T GetNode<T>() where T : Node
        {
            return _graph.Nodes.Find(x => x.GetType() == typeof(T)) as T;
        }

        public Node GetNode(string nodeId)
        {
            return _graph.Nodes.Find(x => x.ID == nodeId);
        }

        public List<T> GetNodes<T>() where T : Node
        {
            return _graph.Nodes.OfType<T>().ToList();
        }

        public List<T> GetNodes<T>(string name) where T : Node
        {
            return GetNodes<T>().Where(x => x.Name == name).ToList();
        }

        public NodeConnection GetConnection(NodePin pin)
        {
            return _graph.Connections.ToList().Where(x => x.SourcePin == pin || x.TargetPin == pin).FirstOrDefault();
        }

        public List<NodeConnection> GetConnections(NodePin pin)
        {
            return _graph.Connections.ToList().Where(x => x.SourcePin == pin || x.TargetPin == pin).ToList();
        }

        public List<NodeConnection> GetConnections(Node node)
        {
            return _graph.Connections.ToList().Where(x => x.SourcePin.Node == node || x.TargetPin.Node == node).ToList();
        }

        public NodeConnection GetConnectionFromStartPin(NodePin startPin)
        {
            return _graph.Connections.ToList().Where(x => x.SourcePin == startPin).FirstOrDefault();
        }

        public NodeConnection GetConnectionFromEndPin(NodePin endPin)
        {
            return _graph.Connections.ToList().Where(x => x.TargetPin == endPin).FirstOrDefault();
        }

        public NodePin GetPin(string nodeId, int pinId)
        {
            var node = GetNode(nodeId);
            if (node != null)
            {
                if (node.HasPin(pinId))
                    return node.Pins[pinId];
            }
            return null;
        }

        public List<NodeVariable> GetNodesByVariableReference(NodeGraphVariable variable)
        {
            return GetNodes<NodeVariable>().Where(x => x.VariableID == variable.ID).ToList();
        }

        public static NodeGraphData GetGraphData(NodeGraph graph)
        {
            NodeEditor.Logger.Log<NodeGraphState>("Serializing graph state...");

            var outGraphData = new NodeGraphData();

            outGraphData.GraphType = graph.GraphType.GetType().ToString();

            // TODO: Find a nicer way to do this...
            graph.Nodes.ForEach(node =>
            {
                if (node.GetType() == typeof(NodeConstant))
                {
                    outGraphData.Constants.Add(NodeConstantData.Convert(node as NodeConstant));
                }
                else if (node.GetType() == typeof(NodeVariable))
                {
                    outGraphData.VariableNodes.Add(NodeVariableData.Convert(node as NodeVariable));
                }
                else
                {
                    outGraphData.Nodes.Add(NodeData.Convert(node));
                }
            });

            graph.Connections.ForEach(connection => outGraphData.Connections.Add(NodeConnectionData.Convert(connection)));
            graph.Variables.ForEach(variable => outGraphData.Variables.Add(NodeGraphVariableData.Convert(variable)));

            return outGraphData;
        }

        public void Dispose()
        {
            VariableAdded = null;
            VariableRemoved = null;
            NodeAdded = null;
            NodeSelected = null;
            NodeRemoved = null;
            _graph = null;
        }
    }
}
