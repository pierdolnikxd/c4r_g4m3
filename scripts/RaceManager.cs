using System.Collections;
using System.Linq;
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
    public bool loopTrack = true;

    [Header("UI Elements")]
    public Text raceInfoText;
    public Text lapText;
    public Text positionText;
    public Text checkpointText;
    public Text bestLapText;
    public Text currentLapTimeText;

    private GameObject playerCar;
    private GameObject aiCar;

    private int currentLap = 0;
    private int currentCheckpoint = 0;
    private bool raceStarted = false;
    private bool raceFinished = false;

    private float lapStartTime;
    private float lapTime;
    private float bestLapTime = Mathf.Infinity;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        AutoDetectCheckpoints();
    }

    void Start()
    {
        FindCars();

        if (raceInfoText != null)
            raceInfoText.text = "Podjedź do punktu startowego, aby rozpocząć wyścig.";
    }

    private void FindCars()
    {
        playerCar = GameObject.FindGameObjectsWithTag("PlayerCar").FirstOrDefault(c => c.activeInHierarchy);
        aiCar = GameObject.FindGameObjectsWithTag("AI").FirstOrDefault();

        if (playerCar == null)
            Debug.LogError("❌ Nie znaleziono aktywnego auta gracza z tagiem 'PlayerCar'!");

        if (aiCar != null)
        {
            aiCar.SetActive(false);
            Debug.Log($"🤖 Auto AI ({aiCar.name}) znalezione i dezaktywowane.");
        }
        else
        {
            Debug.LogWarning("⚠ Nie znaleziono aktywnego auta AI z tagiem 'AI'.");
        }
    }

    private void AutoDetectCheckpoints()
    {
        var cps = FindObjectsOfType<Checkpoint>();
        if (cps.Length == 0)
        {
            Debug.LogWarning("RaceManager: Nie znaleziono żadnych checkpointów!");
            checkpoints = new Transform[0];
            return;
        }

        checkpoints = cps
            .OrderBy(cp => cp.checkpointIndex)
            .Select(cp => cp.transform)
            .ToArray();

        Debug.Log($"RaceManager: Załadowano {checkpoints.Length} checkpointów.");
    }

    public IEnumerator StartRace()
    {
        if (raceStarted) yield break;

        raceStarted = true;
        currentLap = 1;
        currentCheckpoint = 0;
        lapStartTime = Time.time;

        raceInfoText?.SetText("Przygotuj się...");
        yield return new WaitForSeconds(1.5f);

        if (playerCar && playerStartPosition)
            TeleportAndResetCar(playerCar, playerStartPosition);
        else
            Debug.LogWarning("⚠ Brak pozycji startowej gracza!");

        if (aiCar && aiStartPosition)
        {
            aiCar.SetActive(true);
            TeleportAndResetCar(aiCar, aiStartPosition);
        }
        else
        {
            Debug.LogWarning("⚠ Brak pozycji startowej AI!");
        }

        UpdateUI();
        raceInfoText?.SetText("🏁 Wyścig rozpoczęty!");
    }

    private void TeleportAndResetCar(GameObject car, Transform target)
    {
        Rigidbody rb = car.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = target.position;
            rb.rotation = target.rotation;
            rb.Sleep();
            rb.WakeUp();
        }
        else
            car.transform.SetPositionAndRotation(target.position, target.rotation);
    }

    public void CheckpointPassed(int checkpointIndex)
    {
        if (!raceStarted || raceFinished) return;
        if (checkpoints == null || checkpoints.Length == 0) return;

        if (checkpointIndex == currentCheckpoint)
        {
            currentCheckpoint++;

            if (currentCheckpoint >= checkpoints.Length)
            {
                currentCheckpoint = 0;
                currentLap++;

                lapTime = Time.time - lapStartTime;
                if (lapTime < bestLapTime)
                    bestLapTime = lapTime;

                lapStartTime = Time.time;

                if (currentLap > totalLaps)
                {
                    FinishRace();
                    return;
                }

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

        raceInfoText?.SetText($"🏆 Wyścig zakończony! Czas: {lapTime:F2}s");
        EndRaceMode();
        UpdateUI();
        Debug.Log("✅ Wyścig zakończony");
    }

    private void Update()
    {
        if (raceStarted && !raceFinished && currentLapTimeText != null)
        {
            float currentLapTime = Time.time - lapStartTime;
            currentLapTimeText.text = $"Czas okrążenia: {currentLapTime:F2}s";
        }
    }

    private void UpdateUI()
    {
        if (lapText != null)
            lapText.text = $"Okrążenie: {currentLap}/{totalLaps}";

        if (positionText != null)
        {
            string position = GetRacePosition();
            positionText.text = $"Pozycja: {position}";
        }

        if (checkpointText != null)
            checkpointText.text = $"Checkpoint: {currentCheckpoint + 1}/{checkpoints.Length}";

        if (bestLapText != null && bestLapTime < Mathf.Infinity)
            bestLapText.text = $"Najlepsze okr.: {bestLapTime:F2}s";
    }

    private string GetRacePosition()
    {
        if (aiCar == null || !aiCar.activeInHierarchy) return "1/1";

        float playerDist = Vector3.Distance(playerCar.transform.position, checkpoints[currentCheckpoint].position);
        float aiDist = Vector3.Distance(aiCar.transform.position, checkpoints[currentCheckpoint].position);

        // Jeśli AI jest bliżej następnego checkpointu, wyprzedza gracza
        return aiDist < playerDist ? "2/2" : "1/2";
    }

    public void EndRaceMode()
    {
        raceStarted = false;
        raceFinished = false;
        currentLap = 1;
        currentCheckpoint = 0;

        if (aiCar) aiCar.SetActive(false);

        if (playerCar)
        {
            var rb = playerCar.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        Debug.Log("Wyścig zakończony — powrót do jazdy swobodnej.");

        if (aiCar != null)
{
    aiCar.SetActive(false);

    var aiController = aiCar.GetComponent<NewAIRacer>();
    if (aiController != null)
        aiController.ResetRaceState(); // 🔁 resetujemy stan AI
}

    }

#if UNITY_EDITOR
private void OnDrawGizmos()
{
    // 🔁 Automatyczne wykrywanie checkpointów w edytorze (nawet bez uruchamiania gry)
    if (checkpoints == null || checkpoints.Length == 0)
    {
        var cps = FindObjectsOfType<Checkpoint>();
        if (cps.Length > 0)
        {
            checkpoints = cps
                .OrderBy(cp => cp.checkpointIndex)
                .Select(cp => cp.transform)
                .ToArray();
        }
    }

    if (checkpoints == null || checkpoints.Length < 2)
        return;

    Gizmos.color = Color.yellow;

    for (int i = 0; i < checkpoints.Length; i++)
    {
        if (checkpoints[i] == null) continue;

        int nextIndex = (i + 1) % checkpoints.Length;
        if (checkpoints[nextIndex] != null)
            Gizmos.DrawLine(checkpoints[i].position, checkpoints[nextIndex].position);

        Gizmos.DrawSphere(checkpoints[i].position, 0.5f);
        UnityEditor.Handles.Label(checkpoints[i].position + Vector3.up * 1.5f, $"CP {i}");
    }
}
#endif

}

public static class UITextExtensions
{
    public static void SetText(this Text text, string value)
    {
        if (text != null) text.text = value;
    }
}
