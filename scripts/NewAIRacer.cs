using UnityEngine;

public class NewAIRacer : MonoBehaviour
{
    // Odniesienia do komponentów
    private CarController carController;
    private Rigidbody rb;
    
    [Header("Współczynniki AI")]
    [Range(0f, 1f)] public float avoidanceWeight = 0.9f; 
    public float rayLength = 25f;                       // Dłuższy zasięg do unikania
    public float maxSpeedAI = 90f;                      // Maksymalna prędkość
    
    [Header("Płynne Celowanie (Spline-like)")]
    public float lookAheadDistance = 15f;    // Jak daleko przed samochodem jest punkt celu
    [Range(1f, 10f)] public float targetRadius = 5f; // Jak blisko celu AI musi się znaleźć
    [Range(5f, 50f)] public float rotationSpeed = 20f; // Szybkość obracania się do celu
    
    [Header("Logika Wyścigu")]
    private int nextCheckpointIndex = 0;
    private int currentLap = 1;
    
    // Punkt docelowy, który AI ma osiągnąć (jest to pozycja obliczona przed samochodem)
    private Vector3 currentTargetPosition; 

    // --- DODANE DLA INSPECTORA ---
    [Header("Status AI (do podglądu)")]
    public int currentGear = 0;
    public float engineRPM = 0f;
    public float currentSpeedKmh = 0f;
    
    // Stan AI
    private enum AIState { Driving, Recovering }; 
    private AIState currentState = AIState.Driving;
    public float stuckThreshold = 2.0f;
    public float recoveryDuration = 1.5f; 
    private float stuckTimer = 0f;
    private float recoveryTimer = 0f;

    void Start()
    {
        carController = GetComponent<CarController>();
        rb = GetComponent<Rigidbody>();

        if (carController == null || rb == null)
        {
            Debug.LogError("SplineAIRacer wymaga CarController i Rigidbody!", this);
            enabled = false;
        }
        
        // Inicjalizacja pierwszego celu (Checkpoint 0)
        UpdateTargetPosition();
    }
    
    void FixedUpdate() 
    {
        float currentSpeedKmh = rb.linearVelocity.magnitude * 3.6f;

        switch (currentState)
        {
            case AIState.Driving:
                HandleStuckCheck(currentSpeedKmh);
                HandleDriving(currentSpeedKmh);
                break;
            case AIState.Recovering:
                HandleRecovery();
                break;
        }
        
        // Zawsze aktualizuj cel w FixedUpdate, gdy nie jesteś w Recovering
        if (currentState != AIState.Recovering)
        {
             UpdateTargetPosition(); 
        }

        // --- AKTUALIZACJA PRĘDKOŚCI I SILNIKA ---
        currentSpeedKmh = rb.linearVelocity.magnitude * 3.6f;

        if (carController != null)
        {
            currentGear = carController.currentGear;   // zakładam, że CarController ma property CurrentGear
            engineRPM = carController.engineRPM;      // zakładam, że CarController ma property EngineRPM
        }

        switch (currentState)
        {
            case AIState.Driving:
                HandleStuckCheck(currentSpeedKmh);
                HandleDriving(currentSpeedKmh);
                break;
            case AIState.Recovering:
                HandleRecovery();
                break;
        }

        if (currentState != AIState.Recovering)
             UpdateTargetPosition(); 
    }
    
    // --- LOGIKA CELOWANIA I JAZDY ---

    // Nowa metoda: Określa, gdzie na ścieżce AI powinno celować (zamiast po prostu do CP)
    private void UpdateTargetPosition()
{
    if (RaceManager.Instance == null || RaceManager.Instance.checkpoints == null || RaceManager.Instance.checkpoints.Length == 0)
        return;

    // Upewnij się, że nextCheckpointIndex jest w zakresie
    nextCheckpointIndex = Mathf.Clamp(nextCheckpointIndex, 0, RaceManager.Instance.checkpoints.Length - 1);

    Transform cpA = RaceManager.Instance.checkpoints[nextCheckpointIndex];
    Transform cpB = RaceManager.Instance.checkpoints[(nextCheckpointIndex + 1) % RaceManager.Instance.checkpoints.Length];

    if (cpA == null || cpB == null)
    {
        // fallback: ustaw cel na pierwszy niepusty checkpoint
        for (int i = 0; i < RaceManager.Instance.checkpoints.Length; i++)
        {
            if (RaceManager.Instance.checkpoints[i] != null)
            {
                currentTargetPosition = RaceManager.Instance.checkpoints[i].position;
                return;
            }
        }
        return;
    }

    Vector3 ab = cpB.position - cpA.position;
    float abLen = ab.magnitude;

    // Ochrona przed dzieleniem przez zero / zbyt krótką odległością
    if (abLen < 0.001f)
    {
        // Jeżeli A i B praktycznie w tym samym miejscu -> celuj w CP A
        currentTargetPosition = cpA.position;
        Debug.DrawLine(transform.position, currentTargetPosition, Color.magenta);
        return;
    }

    Vector3 dirAB = ab / abLen;

    // Zamiast celować dokładnie w CP A, celujemy 'lookAheadDistance' dalej wzdłuż od A do B
    currentTargetPosition = cpA.position + dirAB * lookAheadDistance;

    // Dodatkowy safeguard: jeżeli lookAhead przesunie poza B, to ustaw cel na B (unikamy "przeskoku")
    float distAtoTarget = Vector3.Distance(cpA.position, currentTargetPosition);
    if (distAtoTarget > abLen)
        currentTargetPosition = cpB.position;

    Debug.DrawLine(transform.position, currentTargetPosition, Color.cyan);
}



    // W NewAIRacer.cs (lub SplineAIRacer.cs)

private void HandleStuckCheck(float currentSpeedKmh)
{
    // Sprawdzenie utknięcia (niska prędkość mimo chęci jazdy)
    // Zakładamy, że CalculateAcceleration zwraca wartość > 0, jeśli auto chce jechać do przodu.
    if (currentSpeedKmh < 5f && CalculateAcceleration(currentSpeedKmh) > 0.1f)
    {
        stuckTimer += Time.fixedDeltaTime;
        if (stuckTimer >= stuckThreshold)
        {
            currentState = AIState.Recovering;
            recoveryTimer = recoveryDuration;
            Debug.Log("AI UTKNĘŁO. Przełączam na COFANIE.");
            stuckTimer = 0f;
        }
    }
    else
    {
        stuckTimer = 0f;
    }
}

// W NewAIRacer.cs (lub SplineAIRacer.cs)

private void HandleRecovery()
{
    // COFANIE: Pełny wsteczny (ujemny gaz) + losowy skręt
    float accelerationInput = -1f; 
    float brakeInput = 0f;
    
    // Zapewniamy losowy skręt, aby ułatwić wydostanie się z zablokowania
    float recoverySteering = Mathf.Sin(recoveryTimer * 10f) * 0.8f; 
    
    // Upewnij się, że używasz wersji SetInputs, która pasuje do Twojego CarController.cs
    // W poprzednich krokach sugerowano wersję z 4 argumentami: (acceleration, steering, brake, handbrake)
    carController.SetInputs(accelerationInput, recoverySteering, brakeInput, false); 
    
    recoveryTimer -= Time.fixedDeltaTime;
    if (recoveryTimer <= 0f)
{
    currentState = AIState.Driving;
    Debug.Log("AI WYSZŁO Z UTKNIĘCIA. Wracam do jazdy.");

    // Przywracamy normalne sterowanie
    UpdateTargetPosition();

    // Nie deklarujemy nowych zmiennych, tylko nadpisujemy istniejące
    accelerationInput = CalculateAcceleration(currentSpeedKmh);
    brakeInput = 0f; 
    float steeringInput = CalculateSteering(); // można zadeklarować nową zmienną, bo jej jeszcze nie ma

    carController.SetInputs(accelerationInput, steeringInput, brakeInput, false);
}

}
    
    private void HandleDriving(float currentSpeedKmh)
    {
        float accelerationInput = CalculateAcceleration(currentSpeedKmh);
        float steeringInput = CalculateSteering();
        float brakeInput = CalculateBrake(currentSpeedKmh, steeringInput);

        // Używamy Lerp do płynnego skręcania w kierunku nowej pozycji
        float finalSteering = Mathf.Lerp(carController.steerInput, steeringInput, Time.fixedDeltaTime * rotationSpeed);

        carController.SetInputs(accelerationInput, finalSteering, brakeInput, false);
    }
    
    private float CalculateSteering()
    {
        // 1. Dążenie do celu (Checkpoint A)
        Vector3 directionToTarget = (currentTargetPosition - transform.position).normalized;
        directionToTarget.y = 0; 
        float angleToTarget = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);

        // Mapowanie kąta na zakres -1 do 1.
        float targetSteering = Mathf.Clamp(angleToTarget / 45f, -1f, 1f); 
        
        // 2. Unikanie przeszkód
        float avoidanceSteering = CalculateAvoidanceSteering(); 

        // 3. Łączenie (unikanie ma priorytet)
        float finalSteering;

        // Jeśli avoidanceSteering jest wystarczająco duże, przejmij sterowanie
        if (Mathf.Abs(avoidanceSteering) > 0.15f) 
        {
            // Pełne przejęcie sterowania + wzmocnienie
            finalSteering = Mathf.Clamp(avoidanceSteering * 1.5f, -1f, 1f); 
        }
        else
        {
            // Płynny miks celu i unikania
            finalSteering = Mathf.Lerp(targetSteering, avoidanceSteering, avoidanceWeight);
        }

        return Mathf.Clamp(finalSteering, -1f, 1f);
    }
    
    // ... (pozostałe metody - CalculateAcceleration, CalculateBrake, CalculateAvoidanceSteering, OnPassCheckpoint) 
    // ... Musisz je skopiować z poprzedniego AIRacer.cs i dostosować.
    
    // Dla uproszczenia, oto tylko kluczowe:

    private float CalculateAcceleration(float currentSpeedKmh)
    {
        float speedRatio = Mathf.Clamp01(currentSpeedKmh / maxSpeedAI);
        float steeringReduction = 1f - Mathf.Abs(CalculateSteering()) * 0.7f; 
        return Mathf.Clamp01(1f - speedRatio) * steeringReduction;
    }

    private float CalculateBrake(float currentSpeedKmh, float steeringInput)
{
    float brake = 0f;

    // Hamowanie w zależności od kąta skrętu
    float angleFactor = Mathf.Abs(steeringInput);
    float speedFactor = currentSpeedKmh / maxSpeedAI;

    brake = Mathf.Clamp01(angleFactor * 0.8f + speedFactor * 0.2f);

    // Dodatkowo hamowanie jeśli punkt docelowy jest blisko
    float distToTarget = Vector3.Distance(transform.position, currentTargetPosition);
    if (distToTarget < 5f)
        brake = Mathf.Max(brake, 0.5f);

    return Mathf.Clamp01(brake);
}

    private float CalculateAvoidanceSteering()
    {
        // Ta metoda powinna być skopiowana z ostatniej, działającej wersji AIRacer.cs,
        // z logiką sumowania Raycastów i silnym wzmocnieniem centralnego promienia.
        return 0f; // Zastąp to pełną logiką Raycastów!
    }
    
    // ... (metody HandleStuckCheck, HandleRecovery, OnPassCheckpoint) ...
    
    // Publiczna metoda wywoływana z Checkpoint.cs
public void OnPassCheckpoint(int passedIndex)
{
    // podstawowa walidacja
    if (RaceManager.Instance == null || RaceManager.Instance.checkpoints == null || RaceManager.Instance.checkpoints.Length == 0)
    {
        Debug.LogWarning("RaceManager lub checkpoints nieprawidłowe w OnPassCheckpoint.");
        return;
    }

    // Jeżeli indeks przekazany z CP jest poza zakresem, zignoruj (może być źle ustawiony w edytorze)
    if (passedIndex < 0 || passedIndex >= RaceManager.Instance.checkpoints.Length)
    {
        Debug.LogWarning($"OnPassCheckpoint: otrzymano nieprawidłowy passedIndex = {passedIndex}");
        return;
    }

    // Normalna walidacja kolejności (zabezpieczenie przed '0' powodującym problemy)
    if (passedIndex == nextCheckpointIndex)
    {
        nextCheckpointIndex = (nextCheckpointIndex + 1) % RaceManager.Instance.checkpoints.Length;

        if (nextCheckpointIndex == 0)
            currentLap++;

        UpdateTargetPosition();
    }
    else
    {
        // Możesz debugować gdy AI zaliczy "nie ten" CP
        Debug.Log($"AI minął CP {passedIndex}, oczekiwano {nextCheckpointIndex}. Ignoruję.");
    }
}

public void ResetRaceState()
{
    nextCheckpointIndex = 0;
    currentLap = 1;
    stuckTimer = 0f;
    recoveryTimer = 0f;
    currentState = AIState.Driving;
    UpdateTargetPosition();
}


}