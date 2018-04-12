using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Framework;

//public class Mine : Hazard, IStateHandler
//{
//    private class StateRecord
//    {
//        public bool Triggered { get; set; }
//    }

//    [SerializeField] private Effect _triggerEffect;

//    private Stack<StateRecord> _stateRecords;
//    private bool _triggered;

//    private void Awake()
//    {
//        _stateRecords = new Stack<StateRecord>();
//    }

//    public override void Initialize(Tile tile, GameboardWorldHelper gameboardHelper)
//    {
//        base.Initialize(tile, gameboardHelper);

//        Assert.IsNotNull(_triggerEffect, "Trigger effect is missing.");

//        Tile.OccupantEntered += Trigger;
//        Tile.ReceivedHealthChange += Trigger;
//    }

//    private void Trigger(Tile tile)
//    {
//        if (_triggered)
//            return;

//        _triggered = true;

//        if (_stateRecords.Count != 0)
//            _stateRecords.Peek().Triggered = true;

//        Effect.Spawn(_triggerEffect, (effect) =>
//        {
//            effect.Apply(Helper, new SpawnEffectParameters(tile, tile));
//        });

//        if (_stateRecords.Count == 0)
//            Remove();
//    }

//    private void Remove()
//    {
//        Tile.OccupantEntered -= Trigger;
//        Tile.ReceivedHealthChange -= Trigger;

//        Tile.RemoveHazard(this);

//        Destroy(gameObject);
//    }

//    void IStateHandler.SaveStateBeforeMove()
//    {
//        _stateRecords.Push(new StateRecord());
//    }

//    void IStateHandler.RestoreStateBeforeMove()
//    {
//        Assert.IsTrue(_stateRecords.Count != 0);
//        _stateRecords.Pop();
//        _triggered = false;
//    }

//    void IStateHandler.CommitStateAfterAttack()
//    {
//        if (_stateRecords.Any(record => record.Triggered))
//            Remove();

//        _stateRecords.Clear();
//    }
//}
