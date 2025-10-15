using UnityEngine;
using System.Linq;

public class AIRacer : MonoBehaviour
{
    // Odniesienia do komponentów
    private CarController carController;
    private Rigidbody rb;
    
    // Zmienne publiczne i konfiguracyjne
    [Header("Współczynniki AI")]
    [Range(0f, 1f)] public float avoidanceWeight = 0.85f; // Waga unikania przeszkód (jak bardzo ma dominować)
    public float rayLength = 15f;                       // Długość promieni Raycast
    public float rayAngle = 20f;                        // Kąt bocznych promieni
    public float maxSpeedAI = 80f;                      // Maksymalna prędkość AI (km/h)

    [Header("Postęp Wyścigu")]
    // Te zmienne są kluczowe do śledzenia postępu AI
    private int nextCheckpointIndex = 0; // Oczekiwany indeks kolejnego checkpointu (od 0 do N-1)
    private int currentLap = 1;          // Aktualne okrążenie AI
    
    // Obiekt docelowy
    private Transform currentTargetTransform;    

    void Start()
    {
        // Sprawdzenie i pobranie komponentów
        carController = GetComponent<CarController>();
        rb = GetComponent<Rigidbody>();

        if (carController == null || rb == null)
        {
            Debug.LogError("AIRacer wymaga CarController i Rigidbody!", this);
            enabled = false;
        }
        
        // Upewnij się, że AI nie jest sterowane domyślnym GetInput()
        // W CarController nie ma publicznej metody do wyłączenia GetInput, ale 
        // możemy polegać na tym, że AIRacer zawsze będzie przekazywał wartości.
    }

    void Update()
    {
        // Musimy odświeżać cel co klatkę, na wypadek, gdyby menedżer wyścigu go zaktualizował
        UpdateTargetCheckpoint();
        
        if (currentTargetTransform == null)
        {
            // Jeśli nie ma celu, zatrzymaj auto (lub jedź na idle)
            carController.SetInputs(0f, 0f, 1f, false);
            return;
        }

        // Główna logika sterowania
        float accelerationInput = CalculateAcceleration();
        float steeringInput = CalculateSteering();
        float brakeInput = 0f; // Będziemy hamować tylko, jeśli cel jest daleko/skręcamy/jest przeszkoda
        bool handbrakeInput = false;

        // Ograniczenie prędkości
        float currentSpeedKmh = rb.linearVelocity.magnitude * 3.6f;
        if (currentSpeedKmh > maxSpeedAI)
        {
            accelerationInput = 0f;
            brakeInput = 0.5f; // Lekkie hamowanie, by zwolnić
        }

        // Hamowanie przy ostrych zakrętach (prosta heurystyka)
        if (Mathf.Abs(steeringInput) > 0.7f && currentSpeedKmh > 50f)
        {
             brakeInput = Mathf.Max(brakeInput, Mathf.Abs(steeringInput) * 0.5f);
        }

        // Przekazanie wartości do CarController
        carController.SetInputs(accelerationInput, steeringInput, brakeInput, handbrakeInput);

    // TEMPORARY DEBUG LOG
    Debug.Log($"AI: Accel={accelerationInput:F2}, Steering={steeringInput:F2}");

    // Przekazanie wartości do CarController
    carController.SetInputs(accelerationInput, steeringInput, brakeInput, handbrakeInput);
    }
    
    // Znajduje i ustawia Transform kolejnego checkpointa
    private void UpdateTargetCheckpoint()
    {
        if (RaceManager.Instance == null || RaceManager.Instance.checkpoints == null || nextCheckpointIndex >= RaceManager.Instance.checkpoints.Length)
        {
            // Koniec toru, nie ma już checkpointów do ustawienia
            currentTargetTransform = null;
            return;
        }
        
        currentTargetTransform = RaceManager.Instance.checkpoints[nextCheckpointIndex];

        if (currentTargetTransform != null)
{
    Debug.DrawLine(transform.position, currentTargetTransform.position, Color.blue); 
}
    }
    


    /// <summary>
    /// Oblicza wartość skręcania na podstawie celu i unikania przeszkód.
    /// Zwraca wartość skręcania (od -1.0f do 1.0f).
    /// </summary>
    private float CalculateSteering()
    {
        // 1. Dążenie do celu (Seek)
        float targetSteering = CalculateTargetSteering();

        // 2. Unikanie przeszkód (Avoidance)
        float avoidanceSteering = CalculateAvoidanceSteering();

        // 3. Połączenie (Weighted Combination)
        // Jeśli wykryto przeszkodę, użyjemy głównie wartości unikania
        if (Mathf.Abs(avoidanceSteering) > 0.05f) 
        {
            // Unikanie dominujące:
            // Łączymy, ale unikaniu dajemy większą wagę
            return (targetSteering * (1f - avoidanceWeight) + avoidanceSteering * avoidanceWeight);
        }
        else
        {
            // Brak przeszkód: Jedź prosto do celu
            return targetSteering;
        }
    }
    
    /// <summary>
    /// Oblicza wartość skręcania potrzebną do dojazdu do aktualnego celu.
    /// </summary>
    private float CalculateTargetSteering()
    {
        if (currentTargetTransform == null) return 0f;

        // Wektor kierunku do celu
        Vector3 directionToTarget = (currentTargetTransform.position - transform.position).normalized;
        directionToTarget.y = 0; // Ignorujemy różnicę wysokości

        // Obliczamy kąt między naszym kierunkiem a kierunkiem do celu
        float angleToTarget = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);

        // Normalizujemy kąt do wartości od -1 (max lewo) do 1 (max prawo)
        // Dzielimy przez maksymalny kąt skrętu (np. 45), aby znormalizować
        return Mathf.Clamp(angleToTarget / 25f, -1f, 1f);
    }
    
    /// <summary>
    /// Używa Raycastów do wykrywania przeszkód i oblicza korektę skrętu.
    /// Zwraca korektę skrętu (od -1.0f do 1.0f).
    /// </summary>
    private float CalculateAvoidanceSteering()
    {
        float avoidance = 0f;
        RaycastHit hit;
        Vector3 forward = transform.forward;
        Vector3 origin = transform.position;

        // Promienie: centralny, lewy, prawy
        Vector3[] rays = new Vector3[]
        {
            forward,
            Quaternion.Euler(0, -rayAngle, 0) * forward, // Lewy
            Quaternion.Euler(0, rayAngle, 0) * forward   // Prawy
        };

        for (int i = 0; i < rays.Length; i++)
        {
            if (Physics.Raycast(origin, rays[i], out hit, rayLength))
            {
                // Jeśli obiekt to checkpoint (lub sam samochód gracza/AI), ignorujemy go
                if (hit.collider.CompareTag("Checkpoint") || hit.collider.CompareTag("PlayerCar") || hit.collider.CompareTag("AI"))
                {
                    continue;
                }

                // Przeszkoda wykryta
                // Im bliżej przeszkoda, tym silniejsza reakcja
                float hitDistanceRatio = 1f - Mathf.Clamp01(hit.distance / rayLength);
                
                if (i == 0) // Promień centralny
                {
                    // Na wprost: skręć tam, gdzie Raycast boczny jest wolny lub w przeciwną stronę
                    // od kierunku, z którego przyszła kolizja (Cross Product z hit.normal)
                    avoidance = (Vector3.Dot(transform.right, hit.normal) > 0) ? -1f : 1f;
                }
                else if (i == 1) // Promień lewy
                {
                    // Przeszkoda po lewej -> skręć mocno w prawo
                    avoidance = 1f * hitDistanceRatio; 
                }
                else if (i == 2) // Promień prawy
                {
                    // Przeszkoda po prawej -> skręć mocno w lewo
                    avoidance = -1f * hitDistanceRatio;
                }
                
                // Rysowanie Raycastów dla debugowania
                Debug.DrawRay(origin, rays[i] * hit.distance, Color.red);
                
                // Zakończ pętlę przy pierwszej wykrytej przeszkodzie dla najsilniejszej reakcji
                return avoidance;
            }
            // Rysowanie Raycastów dla debugowania
            Debug.DrawRay(origin, rays[i] * rayLength, Color.yellow);
        }

        return 0f;
    }
    
    /// <summary>
    /// Utrzymanie stałej prędkości w trakcie jazdy do celu.
    /// </summary>
    private float CalculateAcceleration()
    {
        // Prosta logika przyspieszania: zawsze pełny gaz
        // Można ją rozbudować o redukcję gazu przy ostrym skręcie.
        float currentSpeedKmh = rb.linearVelocity.magnitude * 3.6f;
        
        // Zmniejsz przyspieszenie, gdy jesteśmy już blisko celu maxSpeedAI
        float speedRatio = Mathf.Clamp01(currentSpeedKmh / maxSpeedAI);
        
        // Redukcja gazu przy dużym skręcie
        float steeringReduction = 1f - Mathf.Abs(CalculateTargetSteering()) * 0.5f;

        return Mathf.Clamp01(1f - speedRatio) * steeringReduction;
    }

    /// <summary>
    /// Wywoływana przez Checkpoint.cs, gdy AI zaliczy checkpoint.
    /// </summary>
    public void OnPassCheckpoint(int passedIndex)
    {
        // Sprawdź, czy właśnie przejechaliśmy oczekiwany checkpoint
        if (passedIndex == nextCheckpointIndex)
        {
            // Zwiększamy oczekiwany indeks
            nextCheckpointIndex++;

            // Logika końca okrążenia, jeśli dotarliśmy do ostatniego checkpointa
            if (nextCheckpointIndex >= RaceManager.Instance.checkpoints.Length)
            {
                nextCheckpointIndex = 0; // Wracamy do pierwszego (indeks 0)
                currentLap++;
                Debug.Log($"AI ukończyło okrążenie {currentLap - 1}. Nowy cel: Checkpoint 0.");
            }
            
            Debug.Log($"AI zaliczyło checkpoint {passedIndex}. Nowy cel: Checkpoint {nextCheckpointIndex}");
        }
        else
        {
            Debug.LogWarning($"AI: Przejechany zły checkpoint! Oczekiwano {nextCheckpointIndex}, zaliczono {passedIndex}.");
        }
    }
}