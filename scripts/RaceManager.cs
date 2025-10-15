using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    [Header("Race Settings")]
    public Transform startLine;
    public Transform playerStartPosition;
    public Transform aiStartPosition;
    public Transform[] checkpoints;
    public int totalLaps = 2;

    [Header("UI Elements")]
    public Text raceInfoText;
    public Text lapText;
    public Text positionText;
    public Text checkpointText;

    // Usunięto tablicę aiCars, ponieważ będziemy używać istniejącego obiektu

    private GameObject playerCar;
    private GameObject aiCar; // Teraz będzie to referencja do istniejącego obiektu AI

    private int currentLap = 0;
    private int currentCheckpoint = 0;
    private bool raceStarted = false;
    private bool raceFinished = false;

    private float lapStartTime;
    private float lapTime;

    void Awake()
    {
        // Upewnienie się, że jest tylko jedna instancja
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 1. Zlokalizuj auto gracza
        GameObject[] playerCars = GameObject.FindGameObjectsWithTag("PlayerCar");
        foreach (var car in playerCars)
        {
            if (car.activeInHierarchy)
            {
                playerCar = car;
                break;
            }
        }
        if (playerCar == null)
        {
            Debug.LogError("❌ Nie znaleziono aktywnego auta gracza z tagiem 'PlayerCar'!");
        }

        // 2. Zlokalizuj auto AI i je dezaktywuj
        GameObject[] aiCars = GameObject.FindGameObjectsWithTag("AI");
        if (aiCars.Length > 0)
        {
            aiCar = aiCars[0]; // Użyj pierwszego znalezionego
            // Dezaktywuj auto AI na starcie
            aiCar.SetActive(false); 
            Debug.Log($"🤖 Auto AI ({aiCar.name}) znalezione i dezaktywowane.");
        }
        else
        {
            Debug.LogWarning("⚠ Nie znaleziono aktywnego auta AI z tagiem 'AI'.");
        }


        if (raceInfoText != null)
            raceInfoText.text = "Podjedź do punktu startowego, aby rozpocząć wyścig.";
    }

    public IEnumerator StartRace()
    {
        if (raceStarted) yield break;

        raceStarted = true;
        currentLap = 1;
        currentCheckpoint = 0;
        lapStartTime = Time.time;

        if (raceInfoText != null)
            raceInfoText.text = "Przygotuj się...";

        yield return new WaitForSeconds(1.5f);

        // --- Ustawienie gracza ---
        if (playerCar != null && playerStartPosition != null)
        {
            TeleportAndResetCar(playerCar, playerStartPosition);
            Debug.Log($"🚗 Gracz przeteleportowany na start: {playerStartPosition.position}");
        }
        else
        {
            Debug.LogWarning("⚠ Nie ustawiono pozycji startowej gracza (playerStartPosition).");
        }

        // --- Ustawienie i Aktywacja AI ---
        if (aiCar != null && aiStartPosition != null)
        {
            // Aktywacja istniejącego obiektu AI
            aiCar.SetActive(true); 
            TeleportAndResetCar(aiCar, aiStartPosition);
            Debug.Log($"🤖 AI samochód aktywowany i ustawiony na pozycji {aiStartPosition.position}");

            // Dodatkowo: po aktywacji, zresetuj stan kontrolera (np. dla AIRacer.cs)
            AIRacer aiController = aiCar.GetComponent<AIRacer>();
            if (aiController != null)
            {
                // Załóżmy, że masz metodę resetującą w AIRacer, jeśli jest potrzebna
                // np. aiController.ResetRaceState(); 
            }
        }
        else
        {
             Debug.LogWarning("⚠ Nie ustawiono auta AI lub pozycji startowej AI.");
        }

        UpdateUI();

        if (raceInfoText != null)
            raceInfoText.text = "🏁 Wyścig rozpoczęty!";
    }

    private void TeleportAndResetCar(GameObject car, Transform targetPos)
    {
        Rigidbody rb = car.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            // Przeniesienie i reset fizyki
            rb.position = targetPos.position;
            rb.rotation = targetPos.rotation;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Spróbuj uśpić i wybudzić, by zresetować stany WheelColliderów
            rb.Sleep();
            rb.WakeUp();
        }
        else
        {
            car.transform.SetPositionAndRotation(targetPos.position, targetPos.rotation);
        }
    }


    public void CheckpointPassed(int checkpointIndex)
    {
        if (!raceStarted || raceFinished) return;

        // ... reszta logiki CheckpointPassed (pozostawiona bez zmian) ...

        Debug.Log($"✅ Checkpoint {checkpointIndex} — aktualny: {currentCheckpoint}, okrążenie: {currentLap}");

        // Jeśli gracz przejechał poprawny checkpoint
        if (checkpointIndex == currentCheckpoint)
        {
            currentCheckpoint++;

            // Jeśli skończył okrążenie
            if (currentCheckpoint >= checkpoints.Length)
            {
                currentCheckpoint = 0;
                currentLap++;

                if (currentLap > totalLaps)
                {
                    FinishRace();
                    return;
                }

                lapStartTime = Time.time;
                Debug.Log($"🏁 Rozpoczęto okrążenie {currentLap}");
            }

            UpdateUI();
        }
        else
        {
            Debug.Log($"⚠ Pominięto lub zła kolejność checkpointów! ({checkpointIndex})");
        }
    }

    private void FinishRace()
    {
        raceFinished = true;
        raceStarted = false;

        lapTime = Time.time - lapStartTime;

        if (raceInfoText != null)
            raceInfoText.text = $"🏆 Wyścig zakończony! Czas: {lapTime:F2}s";

        // Deaktywacja AI po zakończeniu wyścigu
        EndRaceMode(); 

        UpdateUI();
        Debug.Log("✅ Wyścig zakończony");
    }

    private void UpdateUI()
    {
        if (lapText != null)
            lapText.text = $"Okrążenie: {currentLap}/{totalLaps}";

        if (positionText != null)
            positionText.text = $"Pozycja: {(aiCar != null && aiCar.activeInHierarchy ? "1/2" : "1/1")}"; // Lepsze sprawdzenie

        if (checkpointText != null)
            checkpointText.text = $"Checkpoint: {currentCheckpoint + 1}/{checkpoints.Length}";

        Debug.Log($"📊 UI zaktualizowane → CP {currentCheckpoint + 1}/{checkpoints.Length}, Lap {currentLap}/{totalLaps}");
    }

    private void OnDrawGizmos()
    {
        if (checkpoints != null && checkpoints.Length > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < checkpoints.Length - 1; i++)
                Gizmos.DrawLine(checkpoints[i].position, checkpoints[i + 1].position);

            if (startLine != null)
                Gizmos.DrawLine(checkpoints[checkpoints.Length - 1].position, startLine.position, 10f); // Dodaj startLine
        }

        if (playerStartPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(playerStartPosition.position, 0.5f);
        }

        if (aiStartPosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(aiStartPosition.position, 0.5f);
        }
    }

    // Nowa metoda do deaktywacji AI i resetu trybu
    public void EndRaceMode()
    {
        raceStarted = false;
        raceFinished = false;
        currentLap = 1;
        currentCheckpoint = 0;

        // Dezaktywuj auto AI
        if (aiCar != null)
        {
             aiCar.SetActive(false);
        }
        
        // Zresetuj fizykę gracza
        if (playerCar != null)
        {
            Rigidbody rb = playerCar.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        Debug.Log("Wyścig zakończony — powrót do jazdy swobodnej.");
    }
}