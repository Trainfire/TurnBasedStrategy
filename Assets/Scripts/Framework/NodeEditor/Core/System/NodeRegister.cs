using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeSystem
{
    public class NodeRegistryEntry
    {
        public Type NodeType { get; private set; }
        public string Name { get; private set; }
        public string Folder { get; private set; }

        public NodeRegistryEntry(Type nodeType, string name, string folder)
        {
            NodeType = nodeType;
            Name = name;
            Folder = folder;
        }
    }

    public class NodeRegistryEntry<T> : NodeRegistryEntry
    {
        public NodeRegistryEntry(string name, string folder) : base(typeof(T), name, folder) { }
    }

    public class NodeRegister
    {
        private List<NodeRegistryEntry> _entries;
        public List<NodeRegistryEntry> Entries { get { return _entries.ToList(); } }

        public NodeRegister()
        {
            _entries = new List<NodeRegistryEntry>();
        }

        public void Add<T>(string name, string folder = "") where T : Node
        {
            _entries.Add(new NodeRegistryEntry(typeof(T), name, folder));
        }
    }

    public static class CoreNodesRegister
    {
        public static NodeRegister Register { get; private set; }

        static CoreNodesRegister()
        {
            Register = new NodeRegister();

            const string conversion = "Conversion";
            Register.Add<ConversionToString<float>>("Float to String", conversion);
            Register.Add<ConversionToString<bool>>("Bool to String", conversion);
            Register.Add<ConversionToString<int>>("Int to String", conversion);

            const string core = "Core";
            Register.Add<CoreDebugLog>("Debug Log", core);

            const string execute = "Execute";
            Register.Add<ExecuteBranch>("Branch", execute);

            const string events = "Events";
            Register.Add<NodeGraphEvent>("Awake", events);
            Register.Add<NodeGraphEvent>("Start", events);
            Register.Add<NodeGraphEvent>("Update", events);

            const string logic = "Logic";
            Register.Add<LogicEquals>("Equals", logic);
            Register.Add<LogicNot>("Not", logic);
            Register.Add<LogicAnd>("And", logic);
            Register.Add<LogicOr>("Or", logic);

            const string math = "Math";
            Register.Add<MathAdd>("Add", math);
            Register.Add<MathSubtract>("Subtract", math);
            Register.Add<MathMultiply>("Multiply", math);
            Register.Add<MathDivide>("Divide", math);

            const string misc = "Misc";
            Register.Add<NodeConstant>("Constant", misc);
            Register.Add<NodeVariable>("Variable", misc);
        }
    }
}
