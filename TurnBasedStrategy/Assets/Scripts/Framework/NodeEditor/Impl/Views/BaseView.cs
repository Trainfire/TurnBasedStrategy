using System;
using NodeSystem;
using UnityEngine;

namespace Framework.NodeEditorViews
{
    public abstract class BaseView : IDisposable
    {
        protected EditorInputListener InputListener { get; private set; }
        protected NodeGraphHelper GraphHelper { get; private set; }

        public BaseView()
        {
            InputListener = new EditorInputListener();
        }

        public void Initialize(NodeGraphHelper graphHelper)
        {
            GraphHelper = graphHelper;
            OnInitialize();
        }

        protected virtual void OnInitialize() { }

        public void Draw()
        {
            InputListener.ProcessEvents();
            OnDraw();
        }

        protected virtual void OnDraw() { }

        public void Dispose()
        {
            OnDispose();
            GraphHelper = null;
            InputListener.Dispose();
        }

        protected virtual void OnDispose() { }
    }
}
