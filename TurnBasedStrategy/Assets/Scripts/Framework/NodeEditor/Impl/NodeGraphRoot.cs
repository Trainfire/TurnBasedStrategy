using UnityEngine;
using NodeSystem;

namespace Framework
{
    public class NodeGraphRoot : MonoBehaviour
    {
        public NodeGraphData GraphData;

        private NodeGraph _graph;
        private NodeGraphRunner _runner;

        void Awake()
        {
            _graph = new NodeGraph();
            _graph.Load(GraphData);

            _runner = new NodeGraphRunner(_graph);
            _runner.RegisterCallback(OnCallback);
            _runner.ExecuteEvent(_graph, "Awake");
        }

        void Start()
        {
            _runner.ExecuteEvent(_graph, "Start");
        }

        void Update()
        {
            _runner.ExecuteEvent(_graph, "Update");
        }

        void OnCallback(Node node)
        {
            var castNode = node as GameObjectNode;
            if (castNode != null)
                castNode.GameObject = gameObject;
        }
    }
}
