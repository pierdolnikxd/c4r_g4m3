using UnityEngine;
using UnityEngine.UI;

public class StartTrigger : MonoBehaviour
{
    public Text promptText;
    private bool canStart = false;

    private RaceManager localRaceManager;

    private void Awake()
    {
        // 🔍 Automatycznie szuka RaceManagera w rodzicu (np. w obiekcie "Race1")
        localRaceManager = GetComponentInParent<RaceManager>();
        if (localRaceManager == null)
            Debug.LogWarning($"⚠ StartTrigger ({name}) nie znalazł RaceManagera w hierarchii nadrzędnej!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerCar"))
        {
            if (promptText != null)
                promptText.text = "Naciśnij ENTER, aby rozpocząć wyścig";
            canStart = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerCar"))
        {
            if (promptText != null)
                promptText.text = "";
            canStart = false;
        }
    }

    private void Update()
    {
        if (canStart && Input.GetKeyDown(KeyCode.Return))
        {
            if (promptText != null)
                promptText.text = "";

            if (localRaceManager != null)
            {
                localRaceManager.StartCoroutine(localRaceManager.StartRace());
            }
            else
            {
                Debug.LogWarning("⚠ StartTrigger: brak referencji do RaceManagera!");
            }
        }
    }
}
