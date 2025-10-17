using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WaypointFollower : MonoBehaviour
{
    public WaypointPath path;
    public float speed = 15f;
    public float turnSpeed = 5f;
    public float waypointRadius = 3f;

    private int currentIndex = 0;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (path == null || path.waypoints.Count == 0)
            Debug.LogError("Brak przypisanej ścieżki waypointów!");
    }

    private void FixedUpdate()
    {
        if (path == null || path.waypoints.Count == 0) return;

        Transform target = path.GetWaypoint(currentIndex);
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;

        // Obrót w stronę waypointa
        Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * turnSpeed);

        // Ruch do przodu
        rb.MovePosition(rb.position + transform.forward * speed * Time.fixedDeltaTime);

        // Przejście do kolejnego waypointa
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist < waypointRadius)
        {
            currentIndex = (currentIndex + 1) % path.waypoints.Count;
        }
    }
}
