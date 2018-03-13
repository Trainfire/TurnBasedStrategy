using UnityEngine;
using NodeSystem;
using System;

namespace Framework
{
    public class GameObjectGraphType : NodeGraphType
    {
        public GameObjectGraphType() : base()
        {
            const string events = "Events";
            RegisterNodeType<NodeGraphEvent>("Awake", events);
            RegisterNodeType<NodeGraphEvent>("Start", events);
            RegisterNodeType<NodeGraphEvent>("Update", events);

            const string transform = "Transform";
            RegisterNodeType<GameObjectGetPosition>("Get Position", transform);
            RegisterNodeType<GameObjectSetPosition>("Set Position", transform);

            const string math = "Math";
            RegisterNodeType<UnityMathSin>("Sin", math);
            RegisterNodeType<UnityMathSplit2>("Split2", math);
            RegisterNodeType<UnityMathSplit3>("Split3", math);
            RegisterNodeType<UnityMathCombine2>("Combine2", math);
            RegisterNodeType<UnityMathCombine3>("Combine3", math);

            const string time = "Time";
            RegisterNodeType<UnityTimeSinceLevelLoad>("Time Since Level Loaded", time);
        }
    }
}
