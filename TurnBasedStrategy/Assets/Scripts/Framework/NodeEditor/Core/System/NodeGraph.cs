using System;
using System.Collections.Generic;
using System.Linq;
using NodeSystem.Editor;

namespace NodeSystem
{
    public class NodeGraph
    {
        public event Action<NodeGraph, NodeGraphData> PostLoad;
        public event Action<NodeGraph> PreUnload;
        public event Action<NodeGraph> PostUnload;
        public event Action<NodeGraph> Edited;

        public Node Selection { get; private set; }
        public NodeGraphState State { get; private set; }

        public event Action<NodeGraphVariable> VariableAdded;
        public event Action<NodeGraphVariable> VariableRemoved;
        public event Action<Node> NodeSelected;
        public event Action<Node> NodeAdded;
        public event Action<Node> NodeRemoved;

        public NodeGraphHelper Helper { get; private set; }
        public List<Node> Nodes { get; private set; }
        public List<NodeConnection> Connections { get; private set; }
        public List<NodeGraphVariable> Variables { get; private set; }
        public NodeGraphType GraphType { get; private set; }

        private NodeGraphType _defaultGraphType;

        public NodeGraph()
        {
            // Default graph type.
            GraphType = new NodeGraphType();

            Nodes = new List<Node>();
            Connections = new List<NodeConnection>();
            Variables = new List<NodeGraphVariable>();
            Helper = new NodeGraphHelper(this);
            State = new NodeGraphState(this);
        }

        public void SetDefaultGraphType<T>(T graphType) where T : NodeGraphType
        {
            _defaultGraphType = graphType;
        }

        public void Load(NodeGraphData graphData)
        {
            NodeEditor.Logger.Log<NodeGraph>("Initializing...");

            Unload();

            NodeEditor.Logger.Log<NodeGraph>("Reading from graph data...");

            if (string.IsNullOrEmpty(graphData.GraphType))
            {
                NodeEditor.Logger.LogWarning<NodeGraph>("Loading graph with no graph type. Defaulting...");
                GraphType = _defaultGraphType;
            }
            else
            {
                var type = Type.GetType(graphData.GraphType);

                NodeEditor.Assertions.IsNotNull(type, "Invalid graph type.");

                if (type != null)
                {
                    GraphType = Activator.CreateInstance(type) as NodeGraphType;
                    NodeEditor.Logger.Log<NodeGraph>("Graph type set to '{0}'", GraphType.GetType().ToString());
                }
                else
                {
                    GraphType = _defaultGraphType;
                    NodeEditor.Logger.Log<NodeGraph>("Graph type invalid. Setting to default.");
                }
            }

            graphData.Variables.ForEach(variable => AddVariable(variable));
            graphData.Nodes.ForEach(x => AddNode(x));
            graphData.Constants.ForEach(x => AddNodeConstant(x));
            graphData.VariableNodes.ForEach(x => AddNodeVariable(x));
            graphData.Connections.ForEach(connectionData => Connect(connectionData));

            if (PostLoad != null)
                PostLoad.Invoke(this, graphData);
        }

        public void Unload()
        {
            PreUnload.InvokeSafe(this);

            Nodes.ToList().ForEach(x => RemoveNode(x));
            Connections.ToList().ForEach(x => Disconnect(x));
            Variables.ToList().ForEach(x => RemoveVariable(x));

            Selection = null;

            PostUnload.InvokeSafe(this);

            NodeEditor.Logger.Log<NodeGraph>("Graph cleared.");
        }

        public void AddVariable(string name, Type type)
        {
            var existingNewVariables = Variables.Where(x => x.Name.Contains(name)).ToList();

            // Add number if an variable exists with the same name.
            if (existingNewVariables.Count != 0)
                name = string.Format("{0} ({1})", name, existingNewVariables.Count + 1);

            AddVariable(new NodeGraphVariableData(name, GetNewGuid(), type));
        }

        NodeGraphVariable AddVariable(NodeGraphVariableData graphVariableData)
        {
            NodeEditor.Assertions.IsFalse(Variables.Any(x => x.ID == graphVariableData.ID), "Tried to spawn a variable that has the same ID as an existing variable.");

            var variable = new NodeGraphVariable(graphVariableData);
            variable.NameChangeRequested += Variable_NameChangeRequested;
            Variables.Add(variable);

            NodeEditor.Logger.Log<NodeGraph>("Added variable '{0}' ({1})", variable.Name, variable.GetType());

            VariableAdded.InvokeSafe(variable);
            Edited.InvokeSafe(this);

            return variable;
        }

        public void RemoveVariable(string variableID)
        {
            NodeEditor.Assertions.IsTrue(Variables.Any(x => x.ID == variableID));
            var variable = Variables.Find(x => x.ID == variableID);
            RemoveVariable(variable);
        }

        public void RemoveVariable(NodeGraphVariable graphVariable)
        {
            NodeEditor.Assertions.IsTrue(Variables.Contains(graphVariable));
            NodeEditor.Logger.Log<NodeGraph>("Removing variable '{0}'", graphVariable.Name);

            Variables.Remove(graphVariable);
            VariableRemoved.InvokeSafe(graphVariable);
            graphVariable.Remove();

            Edited.InvokeSafe(this);
        }

        public void AddNode(NodeRegistryEntry registryEntry)
        {
            if (!GraphType.NodeRegister.Contains(registryEntry))
            {
                NodeEditor.Logger.LogWarning<NodeGraph>("Cannot spawn node of type '{0}' as it is not registered in the graph.", registryEntry.Name);
                return;
            }

            AddNode(registryEntry.NodeType, registryEntry.Name);
        }

        public void AddNode(Type type, string name = "")
        {
            var node = CreateNodeInstance(type);
            node.Initialize(new NodeData(type, GetNewGuid(), name));
        }

        public void AddNode<T>(string name = "") where T : Node
        {
            var node = CreateNodeInstance<T>();
            node.Initialize(new NodeData(typeof(T), GetNewGuid(), name));
        }

        public NodeVariable AddNodeVariable(AddNodeVariableArgs addNodeVariableArgs)
        {
            var variableData = new NodeVariableData(addNodeVariableArgs.Variable, addNodeVariableArgs.AccessorType, GetNewGuid());

            var node = CreateNodeInstance(typeof(NodeVariable)) as NodeVariable;
            node.Initialize(variableData);
            node.Set(addNodeVariableArgs.Variable, addNodeVariableArgs.AccessorType);

            return node;
        }

        Node AddNode(NodeData nodeData)
        {
            var spawnType = Type.GetType(nodeData.ClassType);
            var nodeInstance = CreateNodeInstance(spawnType);
            nodeInstance.Initialize(nodeData);
            return nodeInstance;
        }

        NodeConstant AddNodeConstant(NodeConstantData nodeConstantData)
        {
            var nodeInstance = AddNode(nodeConstantData) as NodeConstant;
            nodeInstance.Set(nodeConstantData);
            return nodeInstance;
        }

        NodeVariable AddNodeVariable(NodeVariableData nodeVariableData)
        {
            var foundVariable = Variables.Find(x => x.ID == nodeVariableData.VariableID);

            NodeEditor.Assertions.WarnIsNotNull(foundVariable, string.Format("The specified variable ({0}) does not exist in the graph.", nodeVariableData.VariableID));

            var nodeInstance = AddNode(nodeVariableData) as NodeVariable;
            nodeInstance.Set(foundVariable, nodeVariableData.AccessorType);
            return nodeInstance;
        }

        Node CreateNodeInstance(Type type)
        {
            var node = Activator.CreateInstance(type) as Node;
            RegisterNode(node);
            return node;
        }

        TNode CreateNodeInstance<TNode>() where TNode : Node
        {
            var node = Activator.CreateInstance<TNode>() as TNode;
            RegisterNode(node);
            return node;
        }

        void RegisterNode(Node node)
        {
            NodeEditor.Assertions.IsFalse(Nodes.Contains(node), "Node already exists in this graph.");

            if (!Nodes.Contains(node))
            {
                NodeEditor.Logger.Log<NodeGraph>("Registered node.");
                node.Destroyed += RemoveNode;
                node.Changed += Node_Changed;
                node.PinRemoved += Node_PinRemoved;

                Nodes.Add(node);
                NodeAdded.InvokeSafe(node);

                Edited.InvokeSafe(this);
            }
        }

        public void RemoveNode(Node node)
        {
            NodeEditor.Assertions.IsTrue(Nodes.Contains(node), "Node Graph does not contain node.");

            if (Nodes.Contains(node))
            {
                NodeEditor.Logger.Log<NodeGraph>("Removing node.");
                Selection = null;
                node.Dispose();
                Nodes.Remove(node);
                NodeRemoved.InvokeSafe(node);

                // TODO: Gracefully handle disconnections...
                var connections = Connections
                    .Where(x => x.SourcePin.Node.ID == node.ID || x.TargetPin.Node.ID == node.ID)
                    .ToList();

                connections.ForEach(x => Disconnect(x));

                Edited.InvokeSafe(this);
            }
        }

        public void Duplicate(List<Node> nodes)
        {
            NodeEditor.Assertions.IsNotNull(nodes);
            nodes.ForEach(node => Duplicate(node));
        }

        public void Duplicate(Node node)
        {
            NodeEditor.Assertions.IsNotNull(node);

            if (node != null)
            {
                var data = NodeData.Convert(node);
                data.ID = GetNewGuid(); // Assign a new ID.

                var duplicate = AddNode(data);
                duplicate.Position += new NodeVec2(5f, 5f);
            }
        }

        /// <summary>
        /// Replaces an existing connection with a new connection. This will create a single state change to the graph.
        /// </summary>
        public void Replace(NodeConnection oldConnection, NodeConnection newConnection)
        {
            if (Connections.Contains(oldConnection))
            {
                NodeEditor.Logger.Log<NodeGraph>("Replacing a connection...");
                Connections.Remove(oldConnection);
                Connect(newConnection);
                NodeEditor.Logger.Log<NodeGraph>("Connection replaced.");
            }
            else
            {
                NodeEditor.Logger.LogWarning<NodeGraph>("Cannot replace connection as the old connection is not a part of this graph.");
            }
        }

        public void Connect(NodeConnectionData connectionData)
        {
            var sourcePin = Helper.GetPin(connectionData.SourceNodeId, connectionData.SourcePinId);
            var targetPin = Helper.GetPin(connectionData.TargetNodeId, connectionData.TargetPinId);

            Connect(new NodeConnection(sourcePin, targetPin));
        }

        public void Connect(NodeConnection connection)
        {
            bool hasConnection = Connections.Contains(connection);

            NodeEditor.Assertions.IsFalse(hasConnection);
            NodeEditor.Assertions.WarnIsNotNull(connection.SourcePin, "Attempted to connect two pins where the start pin was null.");
            NodeEditor.Assertions.WarnIsNotNull(connection.TargetPin, "Attempted to connect two pins where the end pin was null.");
            NodeEditor.Assertions.WarnIsFalse(connection.SourcePin == connection.TargetPin, "Attempted to connect a pin to itself.");
            //Assert.IsFalse(connection.StartPin.WillPinConnectionCreateCircularDependency(connection.EndPin), "Pin connection would create a circular dependency!");

            var canConnect = connection.SourcePin != null 
                && connection.TargetPin != null 
                && connection.SourcePin != connection.TargetPin;

            if (canConnect)
            {
                var existingConnectionOnStartPin = Helper.GetConnections(connection.SourcePin);
                var existingConnectionOnEndPin = Helper.GetConnections(connection.TargetPin);

                if (connection.Type == NodeConnectionType.Execute)
                {
                    // Only one connection is allowed between any two execute pins.
                    existingConnectionOnStartPin.ForEach(x => Connections.Remove(x));
                    existingConnectionOnEndPin.ForEach(x => Connections.Remove(x));
                }
                else if (connection.Type == NodeConnectionType.Value)
                {
                    // Input pins on value types can never have multiple connections. Remove existing connections.
                    existingConnectionOnEndPin.ForEach(x => Connections.Remove(x));
                }

                var startPinId = connection.SourcePin.Index;
                var endPinId = connection.TargetPin.Index;

                connection.SourcePin.Connect(connection.TargetPin);
                connection.TargetPin.Connect(connection.SourcePin);

                // Check if the pin has changed after connecting. This will happen for dynamic pin types.
                if (connection.SourcePin.Node.Pins[startPinId] != connection.SourcePin || connection.TargetPin.Node.Pins[endPinId] != connection.TargetPin)
                    connection = new NodeConnection(connection.LeftNode.Pins[startPinId], connection.RightNode.Pins[endPinId]);

                Connections.Add(connection);

                NodeEditor.Logger.Log<NodeGraph>("Connected {0}:(1) to {2}:{3}", connection.LeftNode.Name, connection.SourcePin.Name, connection.RightNode.Name, connection.TargetPin.Name);

                Edited.InvokeSafe(this);
            }
        }

        public void Disconnect(NodeConnection connection)
        {
            bool containsConnection = Connections.Contains(connection);

            NodeEditor.Assertions.IsTrue(containsConnection);

            if (containsConnection)
            {
                Connections.Remove(connection);
                NodeEditor.Logger.Log<NodeGraph>("Disconnected {0} from {1}.", connection.SourcePin.Node.Name, connection.TargetPin.Node.Name);
                Edited.InvokeSafe(this);
            }
        }

        public void Disconnect(NodePin pin)
        {
            var connection = Connections
                .Where(x => x.LeftNode == pin.Node && x.SourcePin == pin || x.RightNode == pin.Node && x.TargetPin == pin)
                .ToList();

            if (connection.Count != 0)
                connection.ForEach(x => Disconnect(x));
        }

        public void SetSelection(Node node)
        {
            if (node != null)
                NodeEditor.Assertions.IsTrue(Nodes.Contains(node));
            Selection = node;
            NodeSelected.InvokeSafe(Selection);
        }

        string GetNewGuid()
        {
            return Guid.NewGuid().ToString();
        }

        #region Callbacks
        void Node_Changed(Node node)
        {
            NodeEditor.Logger.Log<NodeGraph>("Node {0} changed.", node.Name);
            Edited.InvokeSafe(this);
        }

        void Node_PinRemoved(NodePin pin)
        {
            Disconnect(pin);
            Edited.InvokeSafe(this);
        }

        void Variable_NameChangeRequested(NodeGraphVariableNameChangeRequestEvent nameChangeEvent)
        {
            if (nameChangeEvent.Variable.Name == nameChangeEvent.ReplacementName)
                return;

            NodeEditor.Logger.Log<NodeGraph>("Update name");

            var nameExists = Variables
                .Where(x => x != nameChangeEvent.Variable)
                .Any(x => x.Name == nameChangeEvent.ReplacementName);

            NodeEditor.Assertions.WarnIsFalse(nameExists, string.Format("A variable already exists with the name '{0}'", nameChangeEvent.ReplacementName));

            if (!nameExists)
                nameChangeEvent.Variable.Name = nameChangeEvent.ReplacementName;
        }
        #endregion
    }
}
