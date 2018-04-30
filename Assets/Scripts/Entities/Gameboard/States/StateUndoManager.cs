using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class StateUndoManager
{
    private class MoveUndoRecord
    {
        public Unit Unit { get; private set; }
        public Tile PreviousTile { get; private set; }

        public MoveUndoRecord(Unit unit, Tile previousTile)
        {
            Unit = unit;
            PreviousTile = previousTile;
        }
    }

    public bool CanUndo { get { return _stack.Count != 0; } }

    private Stack<MoveUndoRecord> _stack = new Stack<MoveUndoRecord>();

    public void SavePosition(Unit unit)
    {
        _stack.Push(new MoveUndoRecord(unit, unit.Tile));
    }

    public void UndoLastMove()
    {
        Assert.IsFalse(_stack.Count == 0);

        var pop = _stack.Pop();
        pop.Unit.MoveTo(pop.PreviousTile);
    }

    public void Clear() => _stack.Clear();
}