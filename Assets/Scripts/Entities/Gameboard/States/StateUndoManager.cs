using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class StateUndoEventArgs
{
    public Unit Unit { get; private set; }
    public Tile PreviousTile { get; private set; }

    public StateUndoEventArgs(Unit unit, Tile previousTile)
    {
        Unit = unit;
        PreviousTile = previousTile;
    }
}

public class StateUndoManager
{
    public bool CanUndo { get { return _stack.Count != 0; } }

    private Stack<StateUndoEventArgs> _stack = new Stack<StateUndoEventArgs>();

    public void SavePosition(Unit unit)
    {
        _stack.Push(new StateUndoEventArgs(unit, unit.Tile));
    }

    public StateUndoEventArgs UndoLastMove()
    {
        Assert.IsFalse(_stack.Count == 0);

        var pop = _stack.Pop();
        pop.Unit.MoveTo(pop.PreviousTile);

        return pop;
    }

    public void Clear() => _stack.Clear();
}