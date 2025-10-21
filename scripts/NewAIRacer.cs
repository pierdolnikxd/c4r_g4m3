using UnityEngine;

public class NewAIRacer : MonoBehaviour
{
    private CarController carController;
    private Rigidbody rb;

    [Header("Trasa jazdy (Waypointy)")]
    public WaypointPath waypointPath;
    public int currentWaypointIndex = 0;
    public float waypointReachRadius = 5f;

    [Header("Parametry AI")]
    public float rotationSpeed = 5f;
    public float lookAheadDistance = 10f;
    public float maxSpeedAI = 120f;
    [Range(0f, 1f)] public float steeringSensitivity = 1f;

    [Header("Logika wyścigu")]
    public int nextCheckpointIndex = 0;
    public int currentLap = 1;

    [Header("Podgląd parametrów (Inspector)")]
    public int currentGear;
    public float currentSpeedKmh;
    public float engineRPM;
    public float throttleInput;
    public float brakeInput;
    public float steerInput;

    [Header("Powiązania")]
    public RaceManager raceManager;

    private Vector3 currentTargetPosition;

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

        currentWaypointIndex = 0;
        UpdateTargetPosition();

        carController.SetInputs(0.3f, 0f, 0f);
        if (carController.currentGear == 0)
            carController.currentGear = 1;
    }

    void FixedUpdate()
{
    if (waypointPath == null || waypointPath.waypoints.Count == 0)
        return;

    currentSpeedKmh = carController.GetCurrentSpeed();
    engineRPM = carController.GetEngineRPM();
    currentGear = carController.currentGear;

    UpdateTargetPosition();

    Vector3 dirToTarget = (currentTargetPosition - transform.position);
    dirToTarget.y = 0;
    dirToTarget.Normalize();

    float angle = Vector3.SignedAngle(transform.forward, dirToTarget, Vector3.up);
    steerInput = Mathf.Clamp(angle / 45f, -1f, 1f) * steeringSensitivity;

    // 🔍 Analiza zakrętów (z wyprzedzeniem)
    float cornerSharpness = CalculateCornerSharpness(lookAheadDistance);

    // Prędkość docelowa zależna od krzywizny drogi
    float targetSpeed = Mathf.Lerp(maxSpeedAI * 0.35f, maxSpeedAI, 1f - cornerSharpness);

    // 🧠 Inteligentne hamowanie
    float speedDiff = currentSpeedKmh - targetSpeed;

    if (speedDiff > 5f)
    {
        brakeInput = Mathf.Clamp01(speedDiff / 60f); // im większa różnica, tym mocniejsze hamowanie
        throttleInput = 0f;
    }
    else
    {
        brakeInput = 0f;
        // Dodaj lekki gaz jeśli prędkość mniejsza od celu
        float speedRatio = Mathf.Clamp01(currentSpeedKmh / targetSpeed);
        throttleInput = Mathf.Lerp(1f, 0.3f, speedRatio);
    }

    carController.SetInputs(throttleInput, steerInput, brakeInput);

    // Debug
    Debug.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * 5f, Color.blue);
    Debug.DrawLine(transform.position + Vector3.up * 0.5f, currentTargetPosition + Vector3.up * 0.5f, Color.cyan);
}


// --- NOWA, bardziej precyzyjna metoda oceny zakrętu ---
private float CalculateCornerSharpness(float distanceAhead)
{
    if (waypointPath == null || waypointPath.waypoints.Count < 4)
        return 0f;

    // Szukamy waypointów w pewnej odległości przed autem (ok. lookAheadDistance)
    Vector3 currentPos = transform.position;
    Vector3 forwardDir = transform.forward;
    float totalDistance = 0f;
    float totalAngle = 0f;

    int i = currentWaypointIndex;
    Vector3 lastPos = waypointPath.waypoints[i].position;

    // Idziemy po waypointach dopóki nie przekroczymy lookAheadDistance
    while (totalDistance < distanceAhead)
    {
        int nextIndex = (i + 1) % waypointPath.waypoints.Count;
        Vector3 nextPos = waypointPath.waypoints[nextIndex].position;
        totalDistance += Vector3.Distance(lastPos, nextPos);

        Vector3 dirA = (lastPos - currentPos).normalized;
        Vector3 dirB = (nextPos - lastPos).normalized;
        float angle = Vector3.Angle(dirA, dirB);

        totalAngle += angle;

        lastPos = nextPos;
        i = nextIndex;

        if (i == currentWaypointIndex)
            break; // zapobiega zapętleniu
    }

    // 🔧 Mapowanie kąta na ostrość zakrętu (0 = prosto, 1 = bardzo ostry)
    float sharpness = Mathf.Clamp01(totalAngle / 180f);
    return sharpness;
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
        }

        currentTargetPosition = currentWP.position;
        currentTargetPosition.y = transform.position.y;
    }

    // ------------------ SYSTEM CHECKPOINTÓW ------------------ //
    public void OnPassCheckpoint(int passedIndex)
{
    var rm = raceManager != null ? raceManager : RaceManager.Instance;

    if (rm == null || rm.checkpoints == null || rm.checkpoints.Length == 0)
        return;

    if (passedIndex < 0 || passedIndex >= rm.checkpoints.Length)
        return;

    if (passedIndex == nextCheckpointIndex)
    {
        nextCheckpointIndex = (nextCheckpointIndex + 1) % rm.checkpoints.Length;

        if (nextCheckpointIndex == 0)
            currentLap++;

        UpdateTargetPosition();
    }
    else
    {
        Debug.Log($"AI minął CP {passedIndex}, oczekiwano {nextCheckpointIndex}. Ignoruję.");
    }
}


    public void ResetRaceState()
    {
        nextCheckpointIndex = 0;
        currentLap = 1;
        currentWaypointIndex = 0;
        UpdateTargetPosition();
    }

    // ------------------ GIZMOS ------------------ //
    private void OnDrawGizmos()
    {
        // Kółko pokazujące kierunek skrętu
        Gizmos.color = Color.white;
        Vector3 pos = transform.position + Vector3.up * 2f;
        Gizmos.DrawWireSphere(pos, 0.5f);

        // Linia pokazująca wychylenie kierownicy
        Vector3 steerDir = Quaternion.Euler(0, steerInput * 45f, 0) * transform.forward;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pos, pos + steerDir * 2f);
    }
}