using UnityEngine;
using UnityEditor;

namespace Framework
{
    [CustomEditor(typeof(Trigger))]
    public class TriggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Trigger"))
            {
                (target as Trigger).Fire();
            }
        }
    }
}
