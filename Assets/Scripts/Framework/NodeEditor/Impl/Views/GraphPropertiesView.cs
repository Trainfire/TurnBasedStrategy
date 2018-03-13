using UnityEngine;
using UnityEditor;
using NodeSystem;
using System;
using System.Collections.Generic;
using NodeSystem.Editor;

namespace Framework.NodeEditorViews
{
    public class GraphPropertiesView : NodeEditorPropertiesPanel
    {
        public event Action<AddGraphVariableEvent> AddVariable;
        public event Action<RemoveGraphVariableEvent> RemoveVariable;
        public event Action<AddNodeVariableArgs> AddVariableNode;

        class VariableViewState
        {
            public bool Foldout { get; set; }
            public bool Focused { get; set; }

            public VariableViewState() { }
        }

        Dictionary<NodeGraphVariable, VariableViewState> _variableViewStates;
        List<Node> _nodes;

        public GraphPropertiesView(NodeGraphHelper graphHelper) : base(graphHelper)
        {
            _variableViewStates = new Dictionary<NodeGraphVariable, VariableViewState>();
            _nodes = new List<Node>();

            graphHelper.VariableAdded += GraphHelper_VariableAdded;
            graphHelper.VariableRemoved += GraphHelper_VariableRemoved;
            graphHelper.NodeAdded += GraphHelper_NodeAdded;
        }

        void GraphHelper_NodeAdded(Node obj)
        {
            _nodes.Add(obj);
        }

        void GraphHelper_VariableRemoved(NodeGraphVariable obj)
        {
            if (_variableViewStates.ContainsKey(obj))
                _variableViewStates.Remove(obj);
        }

        void GraphHelper_VariableAdded(NodeGraphVariable obj)
        {
            if (!_variableViewStates.ContainsKey(obj))
                _variableViewStates.Add(obj, new VariableViewState());
        }

        public override void Draw()
        {
            GUILayout.Label("Variables", EditorStyles.boldLabel);

            GraphHelper.Variables.ForEach(x => DrawVariable(x));

            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add New Variable"))
                AddVariable.InvokeSafe(new AddGraphVariableEvent(typeof(float)));

            GUILayout.EndHorizontal();
        }

        void DrawVariable(NodeGraphVariable variable)
        {
            if (!_variableViewStates.ContainsKey(variable))
                return;

            var foldOut = _variableViewStates[variable].Foldout;
            _variableViewStates[variable].Foldout = EditorGUILayout.Foldout(foldOut, variable.Name);

            if (foldOut)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginVertical();

                variable.ReplacementName = EditorGUILayout.DelayedTextField("Name", variable.Name);
                NodeEditorPropertiesHelper.DrawTypeField(variable, GraphHelper.GraphType);
                NodeEditorPropertiesHelper.DrawValueWrapperField(variable.WrappedValue);

                EditorGUILayout.BeginHorizontal();

                GUILayout.Label("Spawn");

                if (GUILayout.Button("Get"))
                    SpawnNodeFromFocusedVariable(variable, NodeGraphVariableAccessorType.Get);

                if (GUILayout.Button("Get Set"))
                    SpawnNodeFromFocusedVariable(variable, NodeGraphVariableAccessorType.GetSet);

                if (GUILayout.Button("Set"))
                    SpawnNodeFromFocusedVariable(variable, NodeGraphVariableAccessorType.Set);

                EditorGUILayout.EndHorizontal();

                // Remove
                if (GUILayout.Button("Remove"))
                {
                    bool doRemoveAction = true;
                    var hasReferences = GraphHelper.GetNodesByVariableReference(variable).Count != 0;

                    if (hasReferences)
                    {
                        doRemoveAction = EditorUtility.DisplayDialog("Warning",
                            "Nodes in the graph are referencing this variable. Are you sure you want to continue?",
                            "Yes",
                            "No");
                    }

                    if (doRemoveAction)
                        RemoveVariable.InvokeSafe(new RemoveGraphVariableEvent(variable));
                }

                EditorGUILayout.EndVertical();

                EditorGUI.indentLevel--;
            }
        }

        void SpawnNodeFromFocusedVariable(NodeGraphVariable variable, NodeGraphVariableAccessorType accessorType)
        {
            AddVariableNode.InvokeSafe(new AddNodeVariableArgs(variable, accessorType));
        }
    }
}
