using UnityEngine;

public class TuningCameraController : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 3f;
    public float zoomSpeed = 10f;
    public float minDistance = 3f;
    public float maxDistance = 10f;

    private float currentDistance;
    private Vector3 currentRotation;

    private void Start()
    {
        if (target == null)
        {
            int selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
            if (GameManager.Instance != null && GameManager.Instance.allCars.Count > selectedCarIndex)
                target = GameManager.Instance.allCars[selectedCarIndex].transform;
        }

        currentDistance = (minDistance + maxDistance) / 2f;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        if (Input.GetMouseButton(0))
        {
            currentRotation.x += Input.GetAxis("Mouse X") * rotationSpeed;
            currentRotation.y -= Input.GetAxis("Mouse Y") * rotationSpeed;
            currentRotation.y = Mathf.Clamp(currentRotation.y, -10f, 60f);
        }

        currentDistance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

        Quaternion rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
        Vector3 position = target.position - rotation * Vector3.forward * currentDistance;

        transform.position = position;
        transform.rotation = rotation;
    }
}
