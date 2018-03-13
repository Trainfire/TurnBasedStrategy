using UnityEngine;
using NodeSystem;

namespace Framework.NodeEditorViews
{
    public class NodeEditorPinView
    {
        public NodePin Pin { get; private set; }
        public Vector2 ScreenPosition { get; private set; }
        public Rect ScreenRect { get; private set; }
        public Rect LocalRect { get; set; }

        public NodeEditorPinView(NodePin pin, Rect localRect)
        {
            Pin = pin;
            LocalRect = localRect;
            ScreenPosition = LocalRect.position + pin.Node.Position.ToVec2();
            ScreenRect = new Rect(ScreenPosition.x, ScreenPosition.y, LocalRect.width, LocalRect.height);
        }
    }
}
