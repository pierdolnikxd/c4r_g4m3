using UnityEngine;

public class AIController : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 50f;
    public float turnSpeed = 5f;
    private int currentWaypoint = 0;

    private void Update()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypoint];
        Vector3 direction = (target.position - transform.position).normalized;

        // Obrót w stronę celu
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);

        // Ruch
        transform.position += transform.forward * speed * Time.deltaTime;

        // Zmiana waypointa
        if (Vector3.Distance(transform.position, target.position) < 10f)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }
}
