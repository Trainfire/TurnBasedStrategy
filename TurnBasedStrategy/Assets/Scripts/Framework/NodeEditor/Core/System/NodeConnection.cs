using NodeSystem.Editor;

namespace NodeSystem
{
    public enum NodeConnectionType
    {
        Execute,
        Value,
    }

    public class NodeConnection
    {
        public bool Hidden { get; private set; }

        public NodePin SourcePin { get; private set; }
        public NodePin TargetPin { get; private set; }
        public Node LeftNode { get { return SourcePin.Node; } }
        public Node RightNode { get { return TargetPin.Node; } }

        public NodeConnectionType Type
        {
            get
            {
                if (SourcePin.IsExecutePin() && TargetPin.IsExecutePin())
                {
                    return NodeConnectionType.Execute;
                }
                else
                {
                    return NodeConnectionType.Value;
                }
            }
        }

        public NodeConnection(NodePin sourcePin, NodePin targetPin)
        {
            NodeEditor.Assertions.WarnIsTrue(sourcePin.IsOutput, "Expected source pin to be an output.");
            NodeEditor.Assertions.WarnIsTrue(targetPin.IsInput, "Expected target pin to be an input.");

            if (sourcePin.IsOutput && targetPin.IsInput)
            {
                // Execute go left to right.
                SourcePin = sourcePin;
                TargetPin = targetPin;
            }
        }

        public void Hide()
        {
            Hidden = true;
        }

        public void Show()
        {
            Hidden = false;
        }
    }
}