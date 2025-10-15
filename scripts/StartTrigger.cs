using UnityEngine;
using UnityEngine.UI;

public class StartTrigger : MonoBehaviour
{
    public Text promptText;
    private bool canStart = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            promptText.text = "Naciśnij ENTER, aby rozpocząć wyścig";
            canStart = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            promptText.text = "";
            canStart = false;
        }
    }

    private void Update()
    {
        if (canStart && Input.GetKeyDown(KeyCode.Return))
        {
            promptText.text = "";
            RaceManager.Instance.StartRace();
        }
    }
}
