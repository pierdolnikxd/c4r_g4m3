using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Mouse Orbit Settings")]
    public float mouseSensitivity = 2f;
    public float minYAngle = -20f;
    public float maxYAngle = 60f;
    public float resetDelay = 1f; // seconds

    private float yaw = 0f;
    private float pitch = 10f;
    private float lastMouseMoveTime = 0f;
    private bool isResetting = false;
    private Vector3 defaultOffset;
    private float defaultYaw;
    private float defaultPitch;
    [Header("Ustawienia Obiektu Śledzonego")]
    [Tooltip("Obiekt, za którym kamera ma podążać (najczęściej samochód)")]
    public Transform target; // Obiekt, za którym kamera ma podążać

    [Header("Ustawienia Pozycji Kamery")]
    [Tooltip("Offset pozycji kamery względem obiektu docelowego (x, y, z)")]
    public Vector3 offset = new Vector3(0f, 1.75f, -4.5f); // Domyślna pozycja kamery za samochodem
    [Tooltip("Prędkość, z jaką kamera podąża za pozycją obiektu (im wyższa, tym szybciej nadgania)")]
    public float followSpeed = 5f; // Jak szybko kamera nadgania pozycję

    [Header("Ustawienia Rotacji Kamery")]
    [Tooltip("Prędkość, z jaką kamera obraca się, aby patrzeć na obiekt (im wyższa, tym szybciej reaguje)")]
    public float rotationSpeed = 15f; // Jak szybko kamera reaguje na obrót obiektu

    void Start()
    {
        // Store default offset and angles
        defaultOffset = offset;
        Vector3 angles = Quaternion.LookRotation(-offset).eulerAngles;
        defaultYaw = angles.y;
        defaultPitch = angles.x;
        yaw = defaultYaw;
        pitch = defaultPitch;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("Brak przypisanego obiektu 'Target' do skryptu CameraFollow!");
            return;
        }

        // Mouse input only when holding left mouse button
        bool mouseHeld = Input.GetMouseButton(0);
        bool rightMouseClicked = Input.GetMouseButtonDown(1);
        float mouseX = mouseHeld ? Input.GetAxis("Mouse X") : 0f;
        float mouseY = mouseHeld ? Input.GetAxis("Mouse Y") : 0f;
        bool mouseMoved = mouseHeld && (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f);

        if (mouseMoved)
        {
            yaw += mouseX * mouseSensitivity;
            pitch -= mouseY * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);
            lastMouseMoveTime = Time.time;
            isResetting = false;
        }

        // Instantly reset camera if right mouse button is clicked
        if (rightMouseClicked)
        {
            yaw = defaultYaw + target.eulerAngles.y;
            pitch = defaultPitch;
            isResetting = false;
        }

        // Calculate camera rotation
        Quaternion camRot = Quaternion.Euler(pitch, yaw + target.eulerAngles.y, 0);
        Vector3 desiredPosition = target.position + camRot * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        Quaternion lookRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }
}