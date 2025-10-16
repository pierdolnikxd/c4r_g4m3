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
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Automatycznie znajdź elementy tylko w ramach tego obiektu (Race1)
        AutoDetectChildren();
    }

    void Start()
    {
        FindCars();

        if (raceInfoText != null)
            raceInfoText.text = "Podjedź do punktu startowego, aby rozpocząć wyścig.";
    }

    // 🔍 Szukanie aut gracza i losowego AI
    private void FindCars()
    {
        playerCar = GameObject.FindGameObjectsWithTag("PlayerCar").FirstOrDefault(c => c.activeInHierarchy);

        var allAICars = GameObject.FindGameObjectsWithTag("AI");
        if (allAICars.Length > 0)
        {
            // Losowo wybieramy jedno AI
            aiCar = allAICars[Random.Range(0, allAICars.Length)];
            aiCar.SetActive(false);
            Debug.Log($"🎲 Wylosowano AI: {aiCar.name}");
        }
        else
        {
            Debug.LogWarning("⚠ Nie znaleziono żadnych samochodów AI w scenie!");
        }

        if (playerCar == null)
            Debug.LogError("❌ Nie znaleziono aktywnego auta gracza z tagiem 'PlayerCar'!");
    }

    // 🔧 Wykrywanie tylko dzieci obiektu Race1 (checkpointy, start line, itp.)
    void AutoDetectChildren()
{
    int count = transform.childCount;
    for (int i = 0; i < count; i++)
    {
        Transform child = transform.GetChild(i);
        Checkpoint cp = child.GetComponent<Checkpoint>();
        if (cp != null)
        {
            cp.SetIndex(i); // ustawia indeks w czasie działania
            Debug.Log($"➜ CP: {child.name} (index {cp.Index}) pod rodzicem {transform.name}");
        }
    }
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

    public void CheckpointPassed(int index)
    {
        if (!raceStarted || raceFinished) return;
        if (checkpoints == null || checkpoints.Length == 0) return;

        if (index == currentCheckpoint)
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
            Debug.Log($"⚠ Pominięto lub zła kolejność checkpointów! ({index})");
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

        return aiDist < playerDist ? "2/2" : "1/2";
    }

    public void EndRaceMode()
    {
        raceStarted = false;
        raceFinished = false;
        currentLap = 1;
        currentCheckpoint = 0;

        if (aiCar != null)
        {
            aiCar.SetActive(false);
            var aiController = aiCar.GetComponent<NewAIRacer>();
            if (aiController != null)
                aiController.ResetRaceState();
        }

        if (playerCar)
        {
            var rb = playerCar.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        Debug.Log("🏁 Wyścig zakończony — powrót do jazdy swobodnej.");
    }

#if UNITY_EDITOR
private void OnDrawGizmos()
{
    // Aktualizuj checkpointy tylko z tego wyścigu i posortuj po index
    checkpoints = GetComponentsInChildren<Checkpoint>(true)
        .OrderBy(cp => cp.index)
        .Select(cp => cp.transform)
        .ToArray();

    if (checkpoints == null || checkpoints.Length < 2)
        return;

    Gizmos.color = Color.yellow;

    for (int i = 0; i < checkpoints.Length; i++)
    {
        if (checkpoints[i] == null) continue;

        // Pobierz środek collidera checkpointa
        Vector3 currentCenter = checkpoints[i].GetComponent<Collider>()?.bounds.center ?? checkpoints[i].position;

        int nextIndex = (i + 1) % checkpoints.Length;
        if (checkpoints[nextIndex] != null)
        {
            Vector3 nextCenter = checkpoints[nextIndex].GetComponent<Collider>()?.bounds.center ?? checkpoints[nextIndex].position;
            Gizmos.DrawLine(currentCenter, nextCenter);
        }

        // Rysuj sferę i label w środku collidera
        Gizmos.DrawSphere(currentCenter, 0.5f);
        UnityEditor.Handles.Label(currentCenter + Vector3.up * 1.5f, $"CP {i}");
    }

    if (startLine != null)
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(startLine.position, new Vector3(5, 0.5f, 1));
        UnityEditor.Handles.Label(startLine.position + Vector3.up * 1.5f, "Start/Meta");
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
