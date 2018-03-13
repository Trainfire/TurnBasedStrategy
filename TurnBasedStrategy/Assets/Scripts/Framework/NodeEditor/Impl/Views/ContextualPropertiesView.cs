using UnityEngine;
using UnityEditor;
using NodeSystem;

namespace Framework.NodeEditorViews
{
    public class ContextualPropertiesView : NodeEditorPropertiesPanel
    {
        public ContextualPropertiesView(NodeGraphHelper graphHelper) : base(graphHelper) { }

        public override void Draw()
        {
            if (GraphHelper.SelectedNode != null)
            {
                EditorGUILayout.LabelField(GraphHelper.SelectedNode.Name);

                if (GraphHelper.SelectedNode.GetType() == typeof(NodeConstant))
                    DrawInspector();
            }
            else
            {
                GUILayout.Label("Nothing selected.", EditorStyles.boldLabel);
            }

            GUILayout.EndVertical();
        }

        void DrawInspector()
        {
            var constant = GraphHelper.SelectedNode as NodeConstant;

            var selectedType = NodeEditorPropertiesHelper.DrawTypeField(constant.ValueWrapper.ValueType, GraphHelper.GraphType, NodePinTypeCategory.Constant);
            constant.SetType(selectedType.WrappedType);

            NodeEditorPropertiesHelper.DrawValueWrapperField(constant.ValueWrapper);
        }
    }
}
