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

    [Header("AI Settings")]
    public GameObject[] aiCars;

    private GameObject playerCar;
    private GameObject aiCar;

    private int currentLap = 0;
    private int currentCheckpoint = 0;
    private bool raceStarted = false;
    private bool raceFinished = false;

    private float lapStartTime;
    private float lapTime;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
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

        // Teleport gracza
        if (playerCar != null && playerStartPosition != null)
        {
            Rigidbody rb = playerCar.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
                rb.position = playerStartPosition.position;
                rb.rotation = playerStartPosition.rotation;
                rb.WakeUp();
            }
            else
            {
                playerCar.transform.SetPositionAndRotation(playerStartPosition.position, playerStartPosition.rotation);
            }

            Debug.Log($"🚗 Gracz przeteleportowany na start: {playerStartPosition.position}");
        }
        else
        {
            Debug.LogWarning("⚠ Nie ustawiono pozycji startowej gracza (playerStartPosition).");
        }

        // Spawn AI
        if (aiCars.Length > 0 && aiStartPosition != null)
        {
            GameObject aiPrefab = aiCars[Random.Range(0, aiCars.Length)];
            aiCar = Instantiate(aiPrefab, aiStartPosition.position, aiStartPosition.rotation);
            Debug.Log($"🤖 AI samochód ustawiony na pozycji {aiStartPosition.position}");
        }

        UpdateUI();

        if (raceInfoText != null)
            raceInfoText.text = "🏁 Wyścig rozpoczęty!";
    }

    public void CheckpointPassed(int checkpointIndex)
    {
        if (!raceStarted || raceFinished) return;

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

        UpdateUI();
        Debug.Log("✅ Wyścig zakończony");
    }

    private void UpdateUI()
    {
        if (lapText != null)
            lapText.text = $"Okrążenie: {currentLap}/{totalLaps}";

        if (positionText != null)
            positionText.text = $"Pozycja: {(aiCar != null ? "1/2" : "1/1")}";

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
                Gizmos.DrawLine(checkpoints[checkpoints.Length - 1].position, startLine.position);
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
    public void ExitRaceMode()
{
    raceStarted = false;
    raceFinished = false;
    currentLap = 1;
    currentCheckpoint = 0;

    // Wznów fizykę i pozwól graczowi ruszyć
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
