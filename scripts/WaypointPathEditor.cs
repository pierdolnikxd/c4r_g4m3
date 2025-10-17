using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaypointPath))]
public class WaypointPathEditor : Editor
{
    private WaypointPath path;
    private bool placingPoints = false;

    private void OnEnable()
    {
        path = (WaypointPath)target;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button(placingPoints ? "🛑 Zakończ rysowanie" : "✏️ Rysuj ścieżkę (Shift + LPM)"))
        {
            placingPoints = !placingPoints;
        }

        if (GUILayout.Button("🧹 Wyczyść ścieżkę"))
        {
            Undo.RecordObject(path, "Clear Waypoints");
            path.waypoints.Clear();
            foreach (Transform child in path.transform)
                DestroyImmediate(child.gameObject);
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!placingPoints) return;

        Event e = Event.current;
        if (e.shift && e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                CreateWaypointAt(hit.point);
                e.Use();
            }
        }
    }

    private void CreateWaypointAt(Vector3 position)
    {
        Undo.RecordObject(path, "Add Waypoint");

        GameObject wp = new GameObject($"Waypoint_{path.waypoints.Count}");
        wp.transform.SetParent(path.transform);
        wp.transform.position = position;

        path.waypoints.Add(wp.transform);
        EditorUtility.SetDirty(path);
    }
}
