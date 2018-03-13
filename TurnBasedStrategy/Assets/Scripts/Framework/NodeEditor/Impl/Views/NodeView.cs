using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Linq;
using System.Collections.Generic;
using NodeSystem;

namespace Framework.NodeEditorViews
{
    public class NodeEditorNodeView : BaseView
    {
        public Node Node { get; private set; }
        public Rect Rect { get; private set; }

        private int _windowId;
        private Dictionary<NodePin, NodeEditorPinView> _pinViews;

        public NodeEditorNodeView(Node node, int windowId)
        {
            Node = node;
            Node.PinAdded += AddPinView;
            Node.PinRemoved += RemovePinView;
            Node.PinChanged += SwapPinView;

            _windowId = windowId;

            _pinViews = new Dictionary<NodePin, NodeEditorPinView>();

            Node.InputPins.ForEach(x => AddPinView(x));
            Node.OutputPins.ForEach(x => AddPinView(x));
        }

        private void SwapPinView(NodePinChangedEvent changedEvent)
        {
            RemovePinView(changedEvent.OldPin);
            AddPinView(changedEvent.NewPin);
        }

        private void RemovePinView(NodePin obj)
        {
            bool containsKey = _pinViews.ContainsKey(obj);
            Assert.IsTrue(containsKey);
            if (containsKey)
                _pinViews.Remove(obj);
        }

        private void AddPinView(NodePin obj)
        {
            bool containsKey = _pinViews.ContainsKey(obj);
            Assert.IsFalse(containsKey);
            if (!containsKey)
                _pinViews.Add(obj, new NodeEditorPinView(obj, new Rect()));
        }

        protected override void OnInitialize()
        {
            InputListener.MouseUp += InputListener_MouseUp;
        }

        protected override void OnDispose()
        {
            _pinViews.Keys.ToList().ForEach(x => RemovePinView(x));
            _pinViews.Clear();
        }

        protected override void OnDraw() { }

        public void Draw(Vector2 offset)
        {
            base.Draw();

            if (Node == null)
                return;

            // NB: A whole bunch of hacks.
            const float nodeWidth = 100f;
            const float headerHeight = 20f;
            const float pinHeight = 20f;
            var height = (Math.Max(Node.InputPins.Count, Node.OutputPins.Count) * pinHeight) + headerHeight;
            var viewSize = new Vector2(nodeWidth, height);

            // Subtract offset due to inverted co-ordinates.
            Rect = new Rect(Node.Position.x - offset.x, Node.Position.y - offset.y, viewSize.x, viewSize.y);
            Rect = GUI.Window(_windowId, Rect, InternalDraw, Node.Name);

            // Set node position to untransformed position.
            Node.Position = (Rect.position + offset).ToNodePosition();
        }

        void InternalDraw(int windowId)
        {
            if (Node == null)
                return;

            GUILayout.BeginHorizontal();

            // Inputs
            GUILayout.BeginVertical();
            _pinViews.Keys.Where(x => x.IsInput).ToList().ForEach(x => DrawPin(x));
            //Node.InputPins.ForEach(x => DrawPin(x));
            GUILayout.EndVertical();

            // Outputs
            GUILayout.BeginVertical();
            _pinViews.Keys.Where(x => x.IsOutput).ToList().ForEach(x => DrawPin(x));
            //Node.OutputPins.ForEach(x => DrawPin(x));
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            if (GetPinUnderMouse() == null)
                GUI.DragWindow();
        }

        public NodeEditorPinView GetPinUnderMouse(Action<NodeEditorPinView> OnPinExists = null)
        {
            NodeEditorPinView pinUnderMouse = null;

            pinUnderMouse = _pinViews
                .Values
                .Where(x => x.ScreenRect.Contains(InputListener.MousePosition))
                .FirstOrDefault();

            if (pinUnderMouse != null)
                OnPinExists.InvokeSafe(pinUnderMouse);

            return pinUnderMouse != null ? pinUnderMouse : null;
        }

        public bool HasPinView(NodePin pin)
        {
            return _pinViews.ContainsKey(pin);
        }

        public NodeEditorPinView GetPinViewData(NodePin pin)
        {
            return _pinViews.ContainsKey(pin) ? _pinViews[pin] : null;
        }

        void DrawPin(NodePin pin)
        {
            var highlighted = _pinViews.ContainsKey(pin) && _pinViews[pin].ScreenRect.Contains(InputListener.MousePosition);
            var drawData = NodeEditorPinDrawer.Draw(pin, highlighted);

            if (Event.current.type == EventType.Repaint && _pinViews.ContainsKey(pin))
                _pinViews[pin] = drawData;
        }

        void InputListener_MouseUp(EditorMouseEvent mouseEvent)
        {
            Node.TriggerPositionChanged();
        }
    }
}
