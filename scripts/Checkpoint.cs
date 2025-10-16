using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex;

    private void OnTriggerEnter(Collider other)
    {
        // Gracz
        if (other.CompareTag("PlayerCar"))
        {
            RaceManager.Instance?.CheckpointPassed(checkpointIndex);
        }
        // AI
        else if (other.CompareTag("AI"))
        {
            NewAIRacer aiRacer = other.GetComponent<NewAIRacer>();
            if (aiRacer != null)
                aiRacer.OnPassCheckpoint(checkpointIndex);
        }
    }

#if UNITY_EDITOR
private void OnDrawGizmos()
{
    Gizmos.color = Color.yellow;

    BoxCollider box = GetComponent<BoxCollider>();
    if (box != null)
    {
        // Ustawienie macierzy Gizmo, aby uwzględniała lokalną pozycję, rotację i skalę
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.matrix = rotationMatrix;

        // Rysowanie pudełka zgodnego z lokalnymi rozmiarami BoxCollidera
        Gizmos.DrawWireCube(box.center, box.size);

        // Obliczamy środek BoxCollidera w world-space
        Vector3 worldCenter = transform.TransformPoint(box.center);

        // Przesunięcie napisu - używamy lokalnej osi 'up' obiektu tak, by etykieta była nad pudłem
        float yOffset = box.size.y * 0.5f + 0.15f; // lekko ponad górną krawędź
        Vector3 labelPos = worldCenter + transform.up * yOffset;

        // Rysujemy etykietę w world-space (Handles.Label działa w przestrzeni światowej)
        UnityEditor.Handles.Label(labelPos, $"CP {checkpointIndex}");
    }
}
#endif


}
