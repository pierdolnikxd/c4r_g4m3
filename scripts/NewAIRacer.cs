using UnityEngine;

public class NewAIRacer : MonoBehaviour
{
    private CarController carController;
    private Rigidbody rb;

    [Header("Trasa jazdy (Waypointy)")]
    public WaypointPath waypointPath;
    public int currentWaypointIndex = 0;
    public float waypointReachRadius = 5f;
    public float lookAheadDistance = 15f;
    [Range(5f, 50f)] public float rotationSpeed = 20f;

    [Header("Współczynniki AI")]
    [Range(0f, 1f)] public float avoidanceWeight = 0.9f; 
    public float rayLength = 25f;
    public float maxSpeedAI = 90f;

    [Header("Logika Wyścigu")]
    public int nextCheckpointIndex = 0;
    public int currentLap = 1;

    [Header("Status AI (do podglądu)")]
    public int currentGear = 0;
    public float engineRPM = 0f;
    public float currentSpeedKmh = 0f;
    public int debugCurrentWaypoint = 0; // <-- widoczny w inspektorze

    [Header("Debug / Wymuszenia")]
    public bool forceFirstGear = false;

    private Vector3 currentTargetPosition;

    private enum AIState { Driving, Recovering };
    private AIState currentState = AIState.Driving;
    private float stuckTimer = 0f;
    private float recoveryTimer = 0f;
    public float stuckThreshold = 2.0f;
    public float recoveryDuration = 1.5f;

    void Start()
{
    carController = GetComponent<CarController>();
    rb = GetComponent<Rigidbody>();

    if (carController == null || rb == null)
    {
        Debug.LogError("NewAIRacer wymaga CarController i Rigidbody!", this);
        enabled = false;
        return;
    }

    if (waypointPath == null || waypointPath.waypoints.Count == 0)
    {
        Debug.LogError("Brak przypisanej ścieżki waypointów!", this);
        enabled = false;
        return;
    }

    // Zawsze zaczynamy od pierwszego waypointa
    currentWaypointIndex = 0;

    // Ustaw cel na pierwszy waypoint
    currentTargetPosition = waypointPath.waypoints[currentWaypointIndex].position;

    // Mała korekta wysokości, by linia celowania była na poziomie auta
    currentTargetPosition.y = transform.position.y;

    // Wymuś przygotowanie auta do startu
    Invoke(nameof(PrepareForRaceStart), 0.05f);
    UpdateTargetPosition();
}

private void PrepareForRaceStart()
{
    // Nadaj minimalny gaz, by CarController wbił bieg 1
    carController.SetInputs(0.2f, 0f, 0f);

    // Jeśli nadal neutral, wymuś bieg 1
    if (carController.currentGear == 0)
    {
        carController.currentGear = 1;
    }
}

void FixedUpdate()
{
    if (waypointPath == null || waypointPath.waypoints.Count == 0)
        return;

    // --- AUTO-START: upewnij się, że jest wbity bieg 1 ---
    if (carController.currentGear == 0 && carController.GetCurrentSpeed() < 1f)
    {
        carController.currentGear = 1;
        carController.SetInputs(0.25f, 0f, 0f);
    }

    UpdateTargetPosition();

    // kierunek do celu
    Vector3 dirToTarget = (currentTargetPosition - transform.position);
    dirToTarget.y = 0;
    dirToTarget.Normalize();

    float angle = Vector3.SignedAngle(transform.forward, dirToTarget, Vector3.up);
    float steerInput = Mathf.Clamp(angle / 45f, -1f, 1f);

    // prędkość
    float throttleInput = 1f;
    float brakeInput = 0f;

    if (Mathf.Abs(angle) > 60f)
    {
        throttleInput = 0.3f;
        brakeInput = 0.2f;
    }

    if (Vector3.Dot(transform.forward, dirToTarget) < -0.2f)
    {
        throttleInput = 0f;
        brakeInput = 1f;
        steerInput = Mathf.Sign(angle);
    }

    carController.SetInputs(throttleInput, brakeInput, steerInput);

    // debug
    Debug.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * 5f, Color.blue);

    currentSpeedKmh = carController.GetCurrentSpeed();
    engineRPM = carController.GetEngineRPM();
    currentGear = carController.currentGear;
    debugCurrentWaypoint = currentWaypointIndex;
}




    // --- AKTUALIZACJA CELU (WAYPOINT + CHECKPOINT) ---
private void UpdateTargetPosition()
{
    if (waypointPath == null || waypointPath.waypoints.Count == 0)
        return;

    // zabezpieczenie indeksu
    if (currentWaypointIndex >= waypointPath.waypoints.Count)
        currentWaypointIndex = 0;

    Transform currentWP = waypointPath.waypoints[currentWaypointIndex];
    if (currentWP == null) return;

    // odległość do waypointa
    float distToWP = Vector3.Distance(transform.position, currentWP.position);

    // przejście do kolejnego waypointa jeśli blisko
    if (distToWP < waypointReachRadius)
    {
        currentWaypointIndex = (currentWaypointIndex + 1) % waypointPath.waypoints.Count;
        currentWP = waypointPath.waypoints[currentWaypointIndex];
        if (currentWP == null) return;
    }

    // cel ustawiamy **dokładnie w miejscu waypointa**
    Vector3 targetPos = currentWP.position;
    targetPos.y = transform.position.y;
    currentTargetPosition = targetPos;

    // debug info
    debugCurrentWaypoint = currentWaypointIndex;

    // cyanowy ray
    Debug.DrawLine(transform.position + Vector3.up * 0.5f, currentTargetPosition + Vector3.up * 0.5f, Color.cyan);
}






    // --- RUCH I ZACHOWANIE ---
    private void HandleDriving(float currentSpeedKmh)
    {
        float accelerationInput = CalculateAcceleration(currentSpeedKmh);
        float steeringInput = CalculateSteering();
        float brakeInput = CalculateBrake(currentSpeedKmh, steeringInput);

        float finalSteering = Mathf.Lerp(carController.steerInput, steeringInput, Time.fixedDeltaTime * rotationSpeed);
        carController.SetInputs(accelerationInput, finalSteering, brakeInput);
    }

    private void HandleStuckCheck(float currentSpeedKmh)
    {
        if (currentSpeedKmh < 5f && CalculateAcceleration(currentSpeedKmh) > 0.1f)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckThreshold)
            {
                currentState = AIState.Recovering;
                recoveryTimer = recoveryDuration;
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    private void HandleRecovery()
    {
        float accelerationInput = -1f;
        float brakeInput = 0f;
        float recoverySteering = Mathf.Sin(recoveryTimer * 10f) * 0.8f;
        carController.SetInputs(accelerationInput, recoverySteering, brakeInput);

        recoveryTimer -= Time.fixedDeltaTime;
        if (recoveryTimer <= 0f)
        {
            currentState = AIState.Driving;
            UpdateTargetPosition();
        }
    }

    // --- LOGIKA STEROWANIA ---
    private float CalculateSteering()
    {
        Vector3 directionToTarget = (currentTargetPosition - transform.position).normalized;
        directionToTarget.y = 0;
        float angleToTarget = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);
        float targetSteering = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);

        float avoidanceSteering = CalculateAvoidanceSteering();
        float finalSteering;

        if (Mathf.Abs(avoidanceSteering) > 0.15f)
            finalSteering = Mathf.Clamp(avoidanceSteering * 1.5f, -1f, 1f);
        else
            finalSteering = Mathf.Lerp(targetSteering, avoidanceSteering, avoidanceWeight);

        return Mathf.Clamp(finalSteering, -1f, 1f);
    }

    private float CalculateAcceleration(float currentSpeedKmh)
    {
        float speedRatio = Mathf.Clamp01(currentSpeedKmh / maxSpeedAI);
        float steeringReduction = 1f - Mathf.Abs(CalculateSteering()) * 0.7f;
        return Mathf.Clamp01(1f - speedRatio) * steeringReduction;
    }

    private float CalculateBrake(float currentSpeedKmh, float steeringInput)
    {
        float brake = 0f;
        float angleFactor = Mathf.Abs(steeringInput);
        float speedFactor = currentSpeedKmh / maxSpeedAI;

        brake = Mathf.Clamp01(angleFactor * 0.8f + speedFactor * 0.2f);

        float distToTarget = Vector3.Distance(transform.position, currentTargetPosition);
        if (distToTarget < 5f)
            brake = Mathf.Max(brake, 0.5f);

        return Mathf.Clamp01(brake);
    }

    private float CalculateAvoidanceSteering()
    {
        float steer = 0f;
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, transform.forward, out hit, rayLength))
            steer -= 0.7f;

        if (Physics.Raycast(origin, transform.right, out hit, rayLength * 0.7f))
            steer -= 0.5f;

        if (Physics.Raycast(origin, -transform.right, out hit, rayLength * 0.7f))
            steer += 0.5f;

        return steer;
    }

    // --- CHECKPOINTS ---
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
        stuckTimer = 0f;
        recoveryTimer = 0f;
        currentState = AIState.Driving;
        currentWaypointIndex = 0;
        UpdateTargetPosition();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (waypointPath == null || waypointPath.waypoints.Count == 0)
            return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < waypointPath.waypoints.Count; i++)
        {
            Transform wp = waypointPath.waypoints[i];
            if (wp == null) continue;

            Gizmos.DrawSphere(wp.position, 0.5f);
            UnityEditor.Handles.Label(wp.position + Vector3.up * 1.5f, $"WP {i}");
        }
    }
#endif
}
