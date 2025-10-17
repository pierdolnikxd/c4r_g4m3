using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WaypointPath : MonoBehaviour
{
    public List<Transform> waypoints = new List<Transform>();
    public Color pathColor = Color.cyan;
    public float gizmoRadius = 0.6f;

    // Rysowanie ścieżki w edytorze
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2)
            return;

        Gizmos.color = pathColor;

        for (int i = 0; i < waypoints.Count; i++)
        {
            Transform current = waypoints[i];
            if (current == null) continue;

            Gizmos.DrawSphere(current.position, gizmoRadius);

            if (i + 1 < waypoints.Count && waypoints[i + 1] != null)
                Gizmos.DrawLine(current.position, waypoints[i + 1].position);
        }
    }

    // Zwraca punkt na ścieżce po indeksie (z zawinięciem)
    public Transform GetWaypoint(int index)
    {
        if (waypoints.Count == 0) return null;
        return waypoints[index % waypoints.Count];
    }
}
