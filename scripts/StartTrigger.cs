using UnityEngine;
using UnityEngine.UI;

public class StartTrigger : MonoBehaviour
{
    public Text promptText;
    private bool canStart = false;

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

            if (RaceManager.Instance != null)
                RaceManager.Instance.StartCoroutine(RaceManager.Instance.StartRace());
        }
    }
}
