using System.Collections.Generic;

public class StateHandledValue<T> : IStateHandler
{
    private Stack<T> _valueStack;

    public T Value { get; set; }

    public StateHandledValue()
    {
        _valueStack = new Stack<T>();
    }

    public void SaveStateBeforeMove()
    {
        _valueStack.Push(Value);
    }

    public void RestoreStateBeforeMove()
    {
        if (_valueStack.Count == 0)
            return;

        Value = _valueStack.Pop();
    }

    public void CommitStateAfterAttack()
    {
        _valueStack.Clear();
    }
}