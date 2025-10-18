using UnityEngine;

public class NewAIRacer : MonoBehaviour
{
    private CarController carController;
    private Rigidbody rb;

    [Header("Trasa jazdy (Waypointy)")]
    public WaypointPath waypointPath;
    public int currentWaypointIndex = 0;
    public float waypointReachRadius = 5f;
    public int nextCheckpointIndex = 0;
    public int currentLap = 1;

    [Header("Parametry ruchu")]
    public float rotationSpeed = 5f;

    [Header("Status AI (do podglądu)")]
    public int currentGear = 0;
    public float engineRPM = 0f;
    public float currentSpeedKmh = 0f;
    public int debugCurrentWaypoint = 0; // <-- widoczny w inspektorze

    private Vector3 currentTargetPosition;

    void Start()
    {
        carController = GetComponent<CarController>();
        rb = GetComponent<Rigidbody>();

        if (carController == null || rb == null)
        {
            Debug.LogError("SimpleAIRacer wymaga CarController i Rigidbody!", this);
            enabled = false;
            return;
        }

        if (waypointPath == null || waypointPath.waypoints.Count == 0)
        {
            Debug.LogError("Brak przypisanej ścieżki waypointów!", this);
            enabled = false;
            return;
        }

        // Zaczynamy od pierwszego waypointa
        currentWaypointIndex = 0;
        UpdateTargetPosition();

        // Wymuś minimalny gaz i bieg 1
        carController.SetInputs(0.2f, 0f, 0f);
        if (carController.currentGear == 0)
            carController.currentGear = 1;
    }

    void FixedUpdate()
    {
        if (waypointPath == null || waypointPath.waypoints.Count == 0)
            return;

            // wymuś bieg 1 jeśli auto stoi
    if (carController.GetCurrentSpeed() < 0.1f && carController.currentGear != 1)
    {
        carController.currentGear = 1;
        carController.SetInputs(0.2f, 0f, 0f); // minimalny gaz
    }

        // Aktualizacja celu
        UpdateTargetPosition();

        // Kierunek do celu
        Vector3 dirToTarget = (currentTargetPosition - transform.position);
        dirToTarget.y = 0;
        dirToTarget.Normalize();

        float angle = Vector3.SignedAngle(transform.forward, dirToTarget, Vector3.up);
        float steerInput = Mathf.Clamp(angle / 45f, -1f, 1f);

        // Ustawiamy pełny gaz, brak hamulca
        float throttleInput = 1f;
        float brakeInput = 0f;

        carController.SetInputs(throttleInput, brakeInput, steerInput);

        // Debug
        Debug.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * 5f, Color.blue);
        debugCurrentWaypoint = currentWaypointIndex;

        currentSpeedKmh = carController.GetCurrentSpeed();
        engineRPM = carController.GetEngineRPM();
        currentGear = carController.currentGear;
    }

    private void UpdateTargetPosition()
    {
        if (currentWaypointIndex >= waypointPath.waypoints.Count)
            currentWaypointIndex = 0;

        Transform currentWP = waypointPath.waypoints[currentWaypointIndex];
        if (currentWP == null) return;

        float distToWP = Vector3.Distance(transform.position, currentWP.position);

        if (distToWP < waypointReachRadius)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypointPath.waypoints.Count;
            currentWP = waypointPath.waypoints[currentWaypointIndex];
            if (currentWP == null) return;
        }

        currentTargetPosition = currentWP.position;
        currentTargetPosition.y = transform.position.y;

        // Cyanowy ray do waypointa
        Debug.DrawLine(transform.position + Vector3.up * 0.5f, currentTargetPosition + Vector3.up * 0.5f, Color.cyan);
    }

    public void OnPassCheckpoint(int passedIndex)
    {
        if (RaceManager.Instance == null || RaceManager.Instance.checkpoints == null || RaceManager.Instance.checkpoints.Length == 0)
            return;

        if (passedIndex < 0 || passedIndex >= RaceManager.Instance.checkpoints.Length)
            return;

        if (passedIndex == nextCheckpointIndex)
        {
            nextCheckpointIndex = (nextCheckpointIndex + 1) % RaceManager.Instance.checkpoints.Length;

            if (nextCheckpointIndex == 0)
                currentLap++;

            UpdateTargetPosition();
        }
    }

    public void ResetRaceState()
    {
        nextCheckpointIndex = 0;
        currentLap = 1;
        currentWaypointIndex = 0;
        UpdateTargetPosition();
    }

}
