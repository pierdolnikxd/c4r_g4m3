using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private RaceManager localRaceManager;

    [SerializeField] public int index = -1; // prywatne, domyślnie -1
    public int Index => index;               // getter tylko do odczytu

    public void SetIndex(int i)
    {
        index = i;
    }

    private void Awake()
    {
        // 🔍 automatycznie znajdź najbliższego RaceManagera w hierarchii nadrzędnej
        localRaceManager = GetComponentInParent<RaceManager>();
        if (localRaceManager == null)
            Debug.LogWarning($"⚠ Checkpoint {name} nie znalazł RaceManagera w rodzicu!");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Gracz
        if (other.CompareTag("PlayerCar"))
        {
            localRaceManager?.CheckpointPassed(index);
        }
        // AI
        else if (other.CompareTag("AI"))
        {
            NewAIRacer aiRacer = other.GetComponent<NewAIRacer>();
            if (aiRacer != null)
                aiRacer.OnPassCheckpoint(index);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.matrix = rotationMatrix;

            Gizmos.DrawWireCube(box.center, box.size);

            Vector3 worldCenter = transform.TransformPoint(box.center);
            float yOffset = box.size.y * 0.5f + 0.15f;
            Vector3 labelPos = worldCenter + transform.up * yOffset;

            UnityEditor.Handles.Label(labelPos, $"CP {index}");
        }
    }
#endif
}
