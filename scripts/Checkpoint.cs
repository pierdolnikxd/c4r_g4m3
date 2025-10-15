using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex;

    private void OnTriggerEnter(Collider other)
    {
        // Sprawdź, czy to jest samochód gracza
        if (other.CompareTag("PlayerCar"))
        {
            if (RaceManager.Instance != null)
            {
                 RaceManager.Instance.CheckpointPassed(checkpointIndex);
            }
        }
        
        // Sprawdź, czy to jest samochód AI (Tag: "AI")
        else if (other.CompareTag("AI"))
        {
            AIRacer aiRacer = other.GetComponent<AIRacer>();
            if (aiRacer != null)
            {
                // Poinformuj skrypt AIRacer, że przejechał ten checkpoint
                aiRacer.OnPassCheckpoint(checkpointIndex);
                
                // Opcjonalnie: Poinformuj RaceManagera o postępach AI (jeśli RaceManager to obsługuje)
                // W Twoim RaceManager.cs nie ma publicznej metody dla AI, więc pomijamy to na razie.
            }
            // Mimo że AI przejechało, RaceManager.CheckpointPassed jest tylko dla gracza,
            // więc nie wywołujemy go dla AI.
        }
    }
}