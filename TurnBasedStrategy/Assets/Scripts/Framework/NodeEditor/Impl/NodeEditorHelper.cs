using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using NodeSystem;
using NodeSystem.Editor;

namespace Framework.NodeEditorViews
{
    public static class NodeEditorExtensions
    {
        public static Vector2 ToVec2(this NodeVec2 nodePosition)
        {
            return new Vector2(nodePosition.x, nodePosition.y);
        }

        public static NodeVec2 ToNodePosition(this Vector2 vec)
        {
            return new NodeVec2(vec.x, vec.y);
        }
    }

    public static class NodeEditorColorHelper
    {
        private static Dictionary<Type, Color> _colorRegistry = new Dictionary<Type, Color>
        {
            { typeof(NodePinTypeNone), Color.white },
            { typeof(NodePinTypeExecute), new Color(0.8f, 0f, 0f) },
            { typeof(float), new Color(0f, 0.8f, 0f) },
            { typeof(int), new Color(0.259f, 0.525f, 0.957f) },
            { typeof(bool), Color.yellow },
            { typeof(string), new Color(0.2f, 0.2f, 0.2f) }
        };

        public static Color GetPinColor(NodePinType type)
        {
            return GetPinColor(type.WrappedType);
        }

        public static Color GetPinColor(Type type)
        {
            return _colorRegistry.ContainsKey(type) ? _colorRegistry[type] : Color.white;
        }
    }

    public static class NodeEditorConnectionDrawer
    {
        public static void Draw(NodeEditorPinView startPin, NodeEditorPinView endPin)
        {
            Draw(GetPinPosition(startPin), GetPinPosition(endPin), NodeEditorColorHelper.GetPinColor(startPin.Pin.WrappedType));
        }

        public static void Draw(NodeEditorPinView startPin, Vector2 endPosition)
        {
            Draw(GetPinPosition(startPin), endPosition, NodeEditorColorHelper.GetPinColor(startPin.Pin.WrappedType));
        }

        public static void Draw(Vector2 start, Vector2 end, Color color)
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

    public static class NodeEditorPinDrawer
    {
        const float PinSize = 10f;

        public static NodeEditorPinView Draw(NodePin pin, bool highlighted)
        {
            GUILayout.BeginHorizontal();

            // Hack to align the element to the right.
            if (!pin.IsInput)
                GUILayout.FlexibleSpace();

            Rect pinRect = new Rect();

            if (pin.IsInput)
            {
                pinRect = DrawPin(pin, highlighted);
                DrawLabel(pin);
            }
            else
            {
                DrawLabel(pin);
                pinRect = DrawPin(pin, highlighted);
            }

            GUILayout.EndHorizontal();

            return new NodeEditorPinView(pin, pinRect);
        }

        static Rect DrawPin(NodePin pin, bool highlighted)
        {
            var startingBg = GUI.backgroundColor;

            var color = NodeEditorColorHelper.GetPinColor(pin.WrappedType);

            if (highlighted)
            {
                var highlightAdd = 0.4f;
                color = new Color(color.r + highlightAdd, color.g + highlightAdd, color.b + highlightAdd);
            }

            GUI.backgroundColor = color;

            GUILayout.Box("", GUILayout.Width(PinSize), GUILayout.Height(PinSize));

            GUI.backgroundColor = startingBg;

            return GUILayoutUtility.GetLastRect();
        }

        static void DrawLabel(NodePin pin)
        {
            GUILayout.Label(new GUIContent(pin.Name, pin.ToString()));
        }
    }

    public static class NodeEditorPropertiesHelper
    {
        public static NodePinType DrawTypeField(Type currentType, NodeGraphType graphType)
        {
            return DrawTypeField(currentType, graphType, graphType.PinTypeRegister.ToList());
        }

        public static NodePinType DrawTypeField(Type currentType, NodeGraphType graphType, params NodePinTypeCategory[] categories)
        {
            return DrawTypeField(currentType, graphType, graphType.GetPins(categories));
        }

        public static void DrawTypeField(NodeGraphVariable graphVariable, NodeGraphType graphType)
        {
            var type = DrawTypeField(graphVariable.WrappedType, graphType).WrappedType;
            graphVariable.SetValueWrapperFromType(type);
        }

        static NodePinType DrawTypeField(Type currentType, NodeGraphType graphType, List<NodePinType> dropdownTypes)
        {
            var pinTypeData = graphType.GetPinType(currentType);

            var typeExists = dropdownTypes.Exists(x => x.WrappedType == pinTypeData.WrappedType);
            NodeEditor.Assertions.IsTrue(typeExists, "The specified type does not exist in the provided list of pin types.");

            if (typeExists)
            {
                var selectedIndex = dropdownTypes.IndexOf(pinTypeData);
                var dropdownOptions = dropdownTypes.Select(x => x.Name).ToArray();

                selectedIndex = EditorGUILayout.Popup("Type", selectedIndex, dropdownOptions);

                return dropdownTypes[selectedIndex];
            }

            return dropdownTypes[0];
        }

        public static void DrawValueWrapperField(NodeValueWrapper valueWrapper)
        {
            var fieldValue = valueWrapper.ToString();

            const string prefix = "Value";

            if (valueWrapper.ValueType == typeof(float))
            {
                var variableAsType = valueWrapper as NodeValueWrapper<float>;
                variableAsType.Set(EditorGUILayout.DelayedFloatField(prefix, variableAsType.Value));
            }
            else if (valueWrapper.ValueType == typeof(int))
            {
                var variableAsType = valueWrapper as NodeValueWrapper<int>;
                variableAsType.Set(EditorGUILayout.DelayedIntField(prefix, variableAsType.Value));
            }
            else if (valueWrapper.ValueType == typeof(bool))
            {
                var variableAsType = valueWrapper as NodeValueWrapper<bool>;
                variableAsType.Set(EditorGUILayout.Toggle(prefix, variableAsType.Value));
            }
            else if (valueWrapper.ValueType == typeof(string))
            {
                var variableAsType = valueWrapper as NodeValueWrapper<string>;
                variableAsType.Set(EditorGUILayout.DelayedTextField(prefix, variableAsType.Value));
            }
        }
    }
}