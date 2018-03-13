using UnityEngine;
using UnityEditor;
using System;
using NodeSystem;
using NodeSystem.Editor;

namespace Framework.NodeEditorViews
{
    public class NodeEditorContextMenuView : BaseView
    {
        public event Action<AddNodeEvent> AddNode;
        public event Action ClearNodes;

        private GenericMenu _menu;

        protected override void OnInitialize()
        {
            InputListener.ContextClicked += OnContextClicked;
            GraphHelper.GraphPostLoaded += GraphHelper_GraphPostLoadComplete;
        }

        void GraphHelper_GraphPostLoadComplete()
        {
            _menu = new GenericMenu();

            GraphHelper.NodeRegister.ForEach(entry =>
            {
                // Format into a folder using slashes, if one is defined.
                var formattedName = entry.Folder != string.Empty ? string.Format("{0}/{1}", entry.Folder, entry.Name) : entry.Name;

                _menu.AddItem(new GUIContent(formattedName), false, () => AddNode.InvokeSafe(new AddNodeEvent(entry)));
            });

            _menu.AddSeparator("");

            _menu.AddItem(new GUIContent("Remove All Nodes"), false, () => ClearNodes.InvokeSafe());
        }

        void OnContextClicked()
        {
            _menu.ShowAsContext();
        }

        protected override void OnDispose()
        {
            AddNode = null;
            ClearNodes = null;
        }
    }
}
