using UnityEngine;
using NodeSystem;

namespace Framework
{
    public class GameObjectNode : Node
    {
        public GameObject GameObject { protected get; set; }
    }

    public class GameObjectGetPosition : GameObjectNode
    {
        private NodePin<Vector3> _out;

        protected override void OnInitialize()
        {
            _out = AddOutputPin<Vector3>("Out");
        }

        public override void Calculate()
        {
            Write(_out, GameObject.transform.position);
        }
    }

    public class GameObjectSetPosition : GameObjectNode, INodeExecuteHandler, INodeExecuteOutput
    {
        private NodePin<Vector3> _inPosition;
        private NodePin<NodePinTypeExecute> _executeIn;

        public NodePin<NodePinTypeExecute> ExecuteOut { get; private set; }

        protected override void OnInitialize()
        {
            _executeIn = AddInputPin<NodePinTypeExecute>("In");
            ExecuteOut = AddOutputPin<NodePinTypeExecute>("Out");
            _inPosition = AddInputPin<Vector3>("Pos");
        }

        public void Execute()
        {
            var inPosition = Read<Vector3>(_inPosition);
            GameObject.transform.position = inPosition;
        }
    }
}