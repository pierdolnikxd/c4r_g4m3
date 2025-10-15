using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("AI"))
        {
            Debug.Log($"{other.name} przejechał checkpoint {checkpointIndex}");
            RaceManager.Instance.CheckpointPassed(other.gameObject, checkpointIndex);
        }
    }
}
