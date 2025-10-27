// NOWA WERSJA MenuCameraController.cs
using UnityEngine;

public class MenuCameraController : MonoBehaviour
{
    [Header("Ustawienia Płynności")]
    [Tooltip("Szybkość przejścia kamery. Wartość 15-20 jest zalecana.")]
    [SerializeField] private float transitionSpeed = 15f; 
    
    // Nie potrzebujesz już tych pól: private float startTime; private bool isMoving = false;
    
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    
    // Ustawia początkowy cel kamery na jej aktualną pozycję
    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    void Update()
    {
        // Płynne przesuwanie w kierunku celu w każdej klatce
        transform.position = Vector3.Lerp(transform.position, targetPosition, transitionSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, transitionSpeed * Time.deltaTime);

        // Opcjonalnie: Log, by potwierdzić, że Update działa
        // Debug.Log($"Kamera aktualizuje pozycję. Cel: {targetPosition}"); 
    }

    public void SetNewTarget(Transform target)
    {
        if (target == null)
        {
             Debug.LogError("MenuCameraController: Cel kamery jest NULL. Nie można rozpocząć ruchu.");
             return;
        }
        
        // Usuwamy isMoving = true, ponieważ ruch jest teraz stały
        targetPosition = target.position;
        targetRotation = target.rotation;
        
        Debug.Log($"MenuCameraController: Rozpoczęto ruch do celu: {target.name}");
    }
}