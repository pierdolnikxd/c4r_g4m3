// MenuCameraController.cs

using UnityEngine;

public class MenuCameraController : MonoBehaviour
{
    [Header("Ustawienia Płynności")]
    // Szybkość przejścia. Wartość 15-20 jest zalecana (0.25 sekundy to ok. 15-20)
    [Tooltip("Szybkość przejścia kamery. Im większa wartość, tym szybciej kamera osiągnie cel.")]
    [SerializeField] private float transitionSpeed = 15f; 
    
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    
    void Start()
    {
        // Inicjalizacja celu na aktualnej pozycji, aby Lerp nie ruszył od razu na (0,0,0)
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    void Update()
    {
        // KLUCZOWA ZMIANA: Ciągłe, płynne przesuwanie w kierunku celu w każdej klatce
        // Ten mechanizm jest bardziej odporny na nadpisywanie
        transform.position = Vector3.Lerp(transform.position, targetPosition, transitionSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, transitionSpeed * Time.deltaTime);
    }

    public void SetNewTarget(Transform target)
    {
        if (target == null)
        {
             Debug.LogError("MenuCameraController: Cel kamery jest NULL. Nie można rozpocząć ruchu.");
             return;
        }
        
        // Aktualizacja celu
        targetPosition = target.position;
        targetRotation = target.rotation;
        
        Debug.Log($"MenuCameraController: Rozpoczęto ruch do celu: {target.name}");
    }
}