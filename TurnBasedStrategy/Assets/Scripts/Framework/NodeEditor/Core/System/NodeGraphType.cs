using NodeSystem.Editor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NodeSystem
{
    public class NodeGraphType : IDisposable
    {
        private List<NodeRegistryEntry> _nodeRegister;
        public ReadOnlyCollection<NodeRegistryEntry> NodeRegister { get { return _nodeRegister.AsReadOnly(); } }

        private Dictionary<Type, NodePinType> _pinTypeRegister;
        public ReadOnlyCollection<NodePinType> PinTypeRegister { get { return _pinTypeRegister.Values.ToList().AsReadOnly(); } }

        public NodeGraphType()
        {
            _nodeRegister = new List<NodeRegistryEntry>();
            _pinTypeRegister = new Dictionary<Type, NodePinType>();

            RegisterPinType<NodePinTypeNone>("None", NodePinTypeCategory.Constant);
            RegisterPinType<NodePinTypeExecute>("Execute", NodePinTypeCategory.Special);
            RegisterPinType<NodePinTypeAny>("Any", NodePinTypeCategory.Special);

            RegisterConstantPinType<float>("Float", typeof(int));
            RegisterConstantPinType<int>("Int");
            RegisterConstantPinType<bool>("Bool");
            RegisterConstantPinType<string>("String");

            const string conversion = "Conversion";
            RegisterNodeType<ConversionToString<float>>("Float to String", conversion);

            const string core = "Core";
            RegisterNodeType<CoreDebugLog>("Debug Log", core);

            const string execute = "Execute";
            RegisterNodeType<ExecuteBranch>("Branch", execute);

            const string logic = "Logic";
            RegisterNodeType<LogicEquals>("Equals", logic);
            RegisterNodeType<LogicNot>("Not", logic);
            RegisterNodeType<LogicAnd>("And", logic);
            RegisterNodeType<LogicOr>("Or", logic);

            const string math = "Math";
            RegisterNodeType<MathAdd>("Add", math);
            RegisterNodeType<MathSubtract>("Subtract", math);
            RegisterNodeType<MathMultiply>("Multiply", math);
            RegisterNodeType<MathDivide>("Divide", math);

            const string misc = "Misc";
            RegisterNodeType<NodeConstant>("Constant", misc);
            RegisterNodeType<NodeVariable>("Variable", misc);
        }

        protected void RegisterNodeType<T>(string name, string folder = "") where T : Node
        {
            _nodeRegister.Add(new NodeRegistryEntry<T>(name, folder));
            NodeEditor.Logger.Log<NodeGraphType>("Registered node type: '{0}'", name);
        }

        protected void RegisterPinType(NodePinType pinType)
        {
            _pinTypeRegister.Add(pinType.WrappedType, pinType);
            NodeEditor.Logger.Log<NodeGraphType>("Registered pin type: '{0}'", pinType.Name);
        }

        protected void RegisterPinType<T>(string name, NodePinTypeCategory category = NodePinTypeCategory.Uncategorized)
        {
            RegisterPinType(new NodePinType(name, typeof(T), category));
        }

        protected void RegisterConstantPinType<T>(string name, params Type[] compatibleTypes)
        {
            RegisterPinType(new NodePinType(name, typeof(T), NodePinTypeCategory.Constant, compatibleTypes));
        }

        public NodePinType GetPinType<T>()
        {
            return GetPinType(typeof(T));
        }

        public NodePinType GetPinType(Type type)
        {
            bool hasType = _pinTypeRegister.ContainsKey(type);
            NodeEditor.Assertions.IsTrue(hasType, "Registry does not contain type");
            return hasType ? _pinTypeRegister[type] : null;
        }

        public List<NodePinType> GetPins(params NodePinTypeCategory[] categories)
        {
            var list = new List<NodePinType>();

            foreach (var category in categories)
            {
                list.AddRange(_pinTypeRegister.Values.Where(x => x.Category == category));
            }

            return list;
        }

        public void Dispose()
        {
            _pinTypeRegister.Clear();
            _nodeRegister.Clear();
            _pinTypeRegister = null;
            _nodeRegister = null;
        }
    }
}
