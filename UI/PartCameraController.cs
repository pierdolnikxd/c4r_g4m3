using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PartCameraController : MonoBehaviour {
    public Transform target; // what to look at
    public float distance = 2f;
    public float minDistance = 0.5f;
    public float maxDistance = 5f;
    public float orbitSpeed = 180f; // degrees/sec
    public float zoomSpeed = 2f;

    private float yaw = 0f;
    private float pitch = 15f;

    void Start() {
        if (target == null) Debug.LogWarning("PartCameraController: no target set.");
        UpdateTransform();
    }

    void Update() {
        if (target == null) return;

        if (Input.GetMouseButton(0)) {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");
            yaw += dx * orbitSpeed * Time.deltaTime;
            pitch -= dy * orbitSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, -85f, 85f);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f) {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        UpdateTransform();
    }

    void UpdateTransform() {
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pos = target.position - rot * Vector3.forward * distance;
        transform.position = pos;
        transform.rotation = rot;
    }
}