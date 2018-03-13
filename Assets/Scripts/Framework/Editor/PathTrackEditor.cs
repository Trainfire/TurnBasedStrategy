using UnityEngine;
using UnityEditor;
using Framework.Components;

[CustomEditor(typeof(PathTrack))]
public class PathTrackEditor : Editor
{
    private readonly Vector2 _buttonOffset = new Vector2(-25f, -25f);
    private const float _buttonSize = 25f;

    void OnSceneGUI()
    {
        var t = target as PathTrack;

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < t.Points.Count; i++)
        {
            t.Points[i] = Handles.PositionHandle(t.Points[i], Quaternion.identity);
        }

        Handles.DrawAAPolyLine(4f, t.Points.ToArray());

        if (EditorGUI.EndChangeCheck())
            Undo.RecordObject(t, "Moved Point");

        // All this UI needs to be draw after for some unknown reason.
        for (int i = 0; i < t.Points.Count; i++)
        {
            var size = new Vector2(_buttonSize * 2f, _buttonSize);

            var worldToScreen = Camera.current.WorldToScreenPoint(t.Points[i]);
            var screenPosition = new Vector2(_buttonOffset.x, -_buttonOffset.y) + new Vector2(worldToScreen.x, Screen.height - worldToScreen.y - size.y * 2f);

            GUILayout.BeginArea(new Rect(screenPosition, size));
            GUILayout.BeginHorizontal();

            // Don't allow the first path to be removed.
            if (i != 0)
            {
                if (GUILayout.Button("-"))
                    t.Remove(i);
            }

            if (GUILayout.Button("+"))
                t.Add(i);

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        if (t.Looped && t.Last() != null && t.First() != null)
            Handles.DrawAAPolyLine(t.Last().Position, t.First().Position);
    }
}
