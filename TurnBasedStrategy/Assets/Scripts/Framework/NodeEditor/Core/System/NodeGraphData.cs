using System;
using System.Collections.Generic;

namespace NodeSystem
{
    [Serializable]
    public class NodeData
    {
        public string ClassType;
        public string Name;
        public string ID;
        public NodeVec2 Position;

        public NodeData() { }

        public NodeData(Type type, string id, string name = "")
        {
            ClassType = type.ToString();
            ID = id;
            Position = new NodeVec2(50f, 50f); // Default position
            Name = name == "" ? "Node" : name;
        }

        public static NodeData Convert(Node node)
        {
            return new NodeData()
            {
                ClassType = node.GetType().ToString(),
                Name = node.Name,
                ID = node.ID,
                Position = node.Position,
            };
        }
    }

    [Serializable]
    public class NodeConstantData : NodeData
    {
        public string ConstantType;
        public string Value;

        public static NodeConstantData Convert(NodeConstant constant)
        {
            // Convert base node data.
            var nodeData = NodeData.Convert(constant);

            var constantData = new NodeConstantData()
            {
                ClassType = nodeData.ClassType,
                Name = nodeData.Name,
                ID = nodeData.ID,
                Position = nodeData.Position,
                ConstantType = constant.ValueWrapper.ValueType.ToString(),
                Value = constant.ValueWrapper.ToString(),
            };

            //switch (constant.PinType)
            //{
            //    case NodeConstantType.None: constantData.Value = string.Empty; break;
            //    case NodeConstantType.Float: constantData.Value = constant.GetFloat().ToString(); break;
            //    case NodeConstantType.Int: constantData.Value = constant.GetInt().ToString(); break;
            //    case NodeConstantType.Bool: constantData.Value = constant.GetBool().ToString(); break;
            //    case NodeConstantType.String: constantData.Value = constant.GetString(); break;
            //};

            return constantData;
        }
    }

    [Serializable]
    public class NodeVariableData : NodeData
    {
        public string VariableID;
        public NodeGraphVariableAccessorType AccessorType;

        public NodeVariableData() { }

        public NodeVariableData(NodeGraphVariable variable, NodeGraphVariableAccessorType accessorType, string id) : base (typeof(NodeVariable), id, "Variable")
        {
            VariableID = variable.ID;
            AccessorType = accessorType;
        }

        public static NodeVariableData Convert(NodeVariable nodeVariable)
        {
            var nodeData = NodeData.Convert(nodeVariable);

            var variableData = new NodeVariableData()
            {
                ClassType = nodeData.ClassType,
                Name = nodeData.Name,
                ID = nodeData.ID,
                Position = nodeData.Position,
                VariableID = nodeVariable.VariableID,
                AccessorType = nodeVariable.AccessorType,
            };

            return variableData;
        }
    }

    [Serializable]
    public class NodePinData
    {
        public string Type;
        public string Name;
        public string ID;
        public int Index;
        public string ConnectedPin;
    }

    [Serializable]
    public class NodeConnectionData
    {
        public string SourceNodeId;
        public int SourcePinId;
        public string TargetNodeId;
        public int TargetPinId;

        public NodeConnectionData(string sourceNodeId, int sourcePinId, string targetNodeId, int targetPinId)
        {
            SourceNodeId = sourceNodeId;
            SourcePinId = sourcePinId;
            TargetNodeId = targetNodeId;
            TargetPinId = targetPinId;
        }

        public static NodeConnectionData Convert(NodeConnection connection)
        {
            return new NodeConnectionData(connection.LeftNode.ID, connection.SourcePin.Index, connection.RightNode.ID, connection.TargetPin.Index);
        }
    }

    [Serializable]
    public class NodeGraphVariableData
    {
        public string Name;
        public string ID;
        public string VariableType;
        public string Value;

        public NodeGraphVariableData() { }

        public NodeGraphVariableData(string name, string id, Type variableType)
        {
            Name = name;
            ID = id;
            VariableType = variableType.ToString();
        }

        public static NodeGraphVariableData Convert(NodeGraphVariable variable)
        {
            return new NodeGraphVariableData
            {
                Name = variable.Name,
                ID = variable.ID,
                VariableType = variable.WrappedType.ToString(),
                Value = variable.WrappedValue.ToString(),
            };
        }
    }

    [Serializable]
    public class NodeGraphData
    {
        public string ID;
        public string GraphType;
        public List<NodeData> Nodes;
        public List<NodeConnectionData> Connections;
        public List<NodeConstantData> Constants;
        public List<NodeGraphVariableData> Variables;
        public List<NodeVariableData> VariableNodes;

        public NodeGraphData()
        {
            ID = "N/A";
            Nodes = new List<NodeData>();
            Connections = new List<NodeConnectionData>();
            Constants = new List<NodeConstantData>();
            Variables = new List<NodeGraphVariableData>();
            VariableNodes = new List<NodeVariableData>();
        }

        public NodeGraphData(NodeGraphData original)
        {
            ID = original.ID;
            GraphType = original.GraphType;
            Nodes = original.Nodes;
            Connections = original.Connections;
            Constants = original.Constants;
            Variables = original.Variables;
            VariableNodes = original.VariableNodes;
        }
    }
}
