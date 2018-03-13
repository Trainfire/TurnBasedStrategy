using UnityEngine;
using UnityEditor;
using System;

namespace Framework.NodeEditorViews
{
    public class NodeEditorMenuStripView : BaseView
    {
        public event Action Save;
        public event Action Revert;
        public event Action Run;

        protected override void OnDraw()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));

            if (!GraphHelper.IsGraphLoaded)
                GUILayout.Label("No graph selected.", EditorStyles.miniLabel);

            if (GraphHelper.IsGraphDirty)
            {
                if (DrawButton("Save"))
                    Save.InvokeSafe();

                if (DrawButton("Revert"))
                    Revert.InvokeSafe();
            }

            if (GraphHelper.IsGraphLoaded)
            {
                if (DrawButton("Run"))
                    Run.InvokeSafe();
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        bool DrawButton(string label)
        {
            return GUILayout.Button(label, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false), GUILayout.MinWidth(100f));
        }

        protected override void OnDispose()
        {
            Save = null;
            Revert = null;
            Run = null;
        }
    }
}
