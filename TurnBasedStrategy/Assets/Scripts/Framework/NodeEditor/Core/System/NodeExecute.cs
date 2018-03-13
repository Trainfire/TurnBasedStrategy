namespace NodeSystem
{
    public interface INodeExecuteOutput
    {
        NodePin<NodePinTypeExecute> ExecuteOut { get; }
    }

    public interface INodeExecuteHandler
    {
        void Execute();
    }

    public abstract class NodeExecute : Node, INodeExecuteHandler, INodeExecuteOutput
    {
        public virtual NodePin<NodePinTypeExecute> ExecuteOut { get; private set; }

        protected override void OnInitialize()
        {
            AddExecuteInPin();
            ExecuteOut = AddExecuteOutPin();
        }

        public abstract void Execute();
    }

    public abstract class NodeExecute1In<T> : NodeExecute
    {
        protected NodePin<T> In { get; private set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            In = AddInputPin<T>("In");
        }

        public override abstract void Execute();
    }

    public abstract class NodeExecute1In1Out<TIn, TOut> : NodeExecute
    {
        protected NodePin<TIn> In { get; private set; }
        protected NodePin<TOut> Out { get; private set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            In = AddInputPin<TIn>("In");
            Out = AddOutputPin<TOut>("Out");
        }

        public override abstract void Execute();
    }
}