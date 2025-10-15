using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    [Header("Ustawienia trasy")]
    public Transform[] checkpoints;
    public int lapsToComplete = 2;

    [Header("UI")]
    public Text lapText;
    public Text positionText;
    public Text timerText;
    public Text debugText;

    [Header("Pojazdy")]
    public GameObject[] aiCars;
    public Transform startPoint;
    public Transform aiStartPoint;
    public GameObject playerCar;

    private GameObject aiInstance;
    private float lapTime;
    private int currentLap = 1;
    private bool raceStarted = false;
    private bool raceFinished = false;

    private int playerNextCheckpoint = 0;
    private int aiNextCheckpoint = 0;

    private GameManager gameManager;
    private CarSelection carSelection;
    private void Start()
{
    gameManager = FindObjectOfType<GameManager>();

    if (gameManager != null)
    {
        // pobierz numer wybranego auta z CarSelection
        int index = CarSelection.selectedCar;

        // upewnij się, że mieści się w zakresie
        if (index >= 0 && index < gameManager.cars.Length)
        {
            playerCar = gameManager.cars[index];
            Debug.Log($"[RaceManager] Wybrane auto gracza: {playerCar.name}");
        }
        else
        {
            Debug.LogWarning("[RaceManager] Niepoprawny indeks wybranego auta!");
        }
    }
    else
    {
        Debug.LogError("[RaceManager] Nie znaleziono GameManagera w scenie!");
    }
}


    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (raceStarted && !raceFinished)
        {
            lapTime += Time.deltaTime;
            timerText.text = $"Czas: {lapTime:F2}s";
        }
        debugText.text = $"Oczekiwany checkpoint: {playerNextCheckpoint}/{checkpoints.Length - 1}";
    }

    private void OnDrawGizmos()
{
    Gizmos.color = Color.yellow;
    Gizmos.DrawSphere(transform.position, 1f);
    UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"Checkpoint {checkpointIndex}");
}


    public void StartRace()
{
    // Losowy pojazd AI
    int randomIndex = Random.Range(0, aiCars.Length);
    aiInstance = Instantiate(aiCars[randomIndex], aiStartPoint.position, aiStartPoint.rotation);
    aiInstance.tag = "AI";

    // 🟩 PRZENIESIENIE GRACZA NA START
    Debug.Log($"[RaceManager] Teleport gracza na: {startPoint.position}");
    Debug.Log($"[RaceManager] Pozycja auta przed teleportem: {playerCar.transform.position}");

    playerCar.transform.SetPositionAndRotation(startPoint.position, startPoint.rotation);

    // Reset prędkości
    Rigidbody rb = playerCar.GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // 🟩 RESET STANU WYŚCIGU
    lapTime = 0f;
    currentLap = 1;
    playerNextCheckpoint = 0;
    raceStarted = true;
    raceFinished = false;

    lapText.text = $"Okrążenie: {currentLap}/{lapsToComplete}";
    positionText.text = "Pozycja: 1/2";
}


    public void CheckpointPassed(GameObject car, int checkpointIndex)
{
    // GRACZ
    if (car.CompareTag("Player"))
    {
        if (checkpointIndex == playerNextCheckpoint)
        {
            playerNextCheckpoint++;

            // Jeśli gracz zaliczył wszystkie checkpointy
            if (playerNextCheckpoint >= checkpoints.Length)
            {
                playerNextCheckpoint = 0;
                currentLap++;

                if (currentLap > lapsToComplete)
                {
                    raceFinished = true;
                    positionText.text = "META!";
                    lapText.text = "Wyścig zakończony!";
                    return;
                }

                lapText.text = $"Okrążenie: {currentLap}/{lapsToComplete}";
                lapTime = 0f;
            }
        }
    }

    // AI
    if (car.CompareTag("AI"))
    {
        if (checkpointIndex == aiNextCheckpoint)
        {
            aiNextCheckpoint++;
            if (aiNextCheckpoint >= checkpoints.Length)
            {
                aiNextCheckpoint = 0;
            }
        }
    }
}

}
