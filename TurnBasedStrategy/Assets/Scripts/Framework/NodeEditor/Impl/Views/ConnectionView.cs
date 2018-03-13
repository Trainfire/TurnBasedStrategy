using UnityEngine;
using UnityEditor;
using System;
using NodeSystem;

namespace Framework.NodeEditorViews
{
    public static class NodeEditorConnectionView
    {
        public static void DrawConnection(NodeEditorPinView startPin, NodeEditorPinView endPin)
        {
            DrawConnection(GetPinPosition(startPin), GetPinPosition(endPin), NodeEditorColorHelper.GetPinColor(startPin.Pin.WrappedType));
        }

        public static void DrawConnection(Vector2 start, Vector2 end, Color color)
        {
            var startTangent = new Vector2(end.x, start.y);
            var endTangent = new Vector2(start.x, end.y);
            Handles.BeginGUI();
            Handles.DrawBezier(start, end, startTangent, endTangent, color, null, 2f);
            Handles.EndGUI();
        }

        public static Vector2 GetPinPosition(NodeEditorPinView pin)
        {
            return (pin.ScreenPosition + new Vector2(0f, pin.LocalRect.height * 0.5f));
        }
    }
}
