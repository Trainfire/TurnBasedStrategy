using UnityEngine;
using UnityEngine.Assertions;
using NodeSystem;
using NodeSystem.Editor;

namespace Framework.NodeEditorViews
{
    public class NodeEditorPinConnectorView : BaseView, INodeEditorPinConnectorView
    {
        public string Tooltip { get; set; }

        public NodePin EndPin
        {
            set
            {
                Assert.IsTrue(_graphView.HasPinView(value));
                _endPin = _graphView.GetPinView(value);
            }
        }

        private bool _isDrawing;
        private NodeEditorPinView _startPin;
        private NodeEditorPinView _endPin;
        private NodeEditorGraphView _graphView;

        public NodeEditorPinConnectorView(NodeEditorGraphView graphView) : base()
        {
            _graphView = graphView;
        }

        public void EnterDrawState(NodePin startPin)
        {
            _isDrawing = true;
            Assert.IsTrue(_graphView.HasPinView(startPin));
            _startPin = _graphView.GetPinView(startPin);
        }

        public void EndDrawState()
        {
            _isDrawing = false;
            _startPin = null;
        }

        protected override void OnDraw()
        {
            if (_isDrawing)
            {
                if (_startPin == null)
                {
                    _isDrawing = false;
                    return;
                }

                NodeEditorConnectionDrawer.Draw(_startPin, InputListener.MousePosition);

                if (_endPin != null)
                {
                    var offset = new Vector2(0, -25f);
                    var rect = new Rect(InputListener.MousePosition + offset, new Vector2(200f, 20f));

                    if (Tooltip != string.Empty)
                    {
                        //GUILayout.BeginArea(rect);
                        //GUILayout.Box(Tooltip);
                        //GUILayout.EndArea();
                    }
                }
            }
        }
    }
}
