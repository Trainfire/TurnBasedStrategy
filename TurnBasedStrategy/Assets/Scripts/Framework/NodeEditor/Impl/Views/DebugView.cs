using UnityEngine;
using UnityEditor;
using NodeSystem;

namespace Framework.NodeEditorViews
{
    public class NodeEditorDebugView : BaseView
    {
        protected override void OnDraw()
        {
            const float height = 150f;
            GUILayout.BeginArea(new Rect(new Vector2(0f, Screen.height - height), new Vector2(Screen.width * 0.5f, height)), EditorStyles.inspectorFullWidthMargins);

            DrawField("Focused UI", GUI.GetNameOfFocusedControl());
            DrawField("Mouse Pos", InputListener.MousePosition);

            DrawHeader("Graph Info");

            if (GraphHelper == null)
            {
                DrawField("No graph loaded");
            }
            else
            {
                DrawField("Node Count", GraphHelper.NodeCount);
            }

            GUILayout.EndArea();

            if (GraphHelper.SelectedNode != null && GraphHelper != null)
            {
                // Node Debug
                GUILayout.BeginArea(new Rect(new Vector2(Screen.width - 400f, Screen.height - height), new Vector2(400f, height)));

                DrawHeader("Selected Node: " + GraphHelper.SelectedNode.Name);

                GraphHelper.SelectedNode.Pins.ForEach(pin =>
                {
                    DrawField(string.Format("{0} (ID: {1}) (Connected: {2}) (Is Input?: {3})", pin.Name, pin.Index, GraphHelper.IsPinConnected(pin), pin.IsInput), pin.ToString());
                });

                GUILayout.EndArea();
            }
        }

        void DrawHeader(string label)
        {
            var guiStyle = new GUIStyle();
            guiStyle.fontStyle = FontStyle.Bold;

            GUILayout.Label(label, guiStyle);
        }

        void DrawField(string label, object value = null)
        {
            string valueStr = string.Empty;

            if (value != null)
                valueStr = value.ToString() == string.Empty ? "N/A" : value.ToString();

            var outStr = value != null ? string.Format("{0}: {1}", label, valueStr) : label;
            GUILayout.Label(outStr);
        }
    }
}