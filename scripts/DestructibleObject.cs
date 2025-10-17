using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [Header("Wymagana siła do przewrócenia")]
    public float requiredImpactVelocity = 5.0f; 

    [Header("Efekt fizyczny")]
    public float impactForceMultiplier = 150.0f;
    [Range(0.0f, 1.0f)]
    public float carSpeedPreservation = 0.95f;

    private Rigidbody rb;
    private bool isKinematicInitially = true; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 100f;
        }
        
        isKinematicInitially = rb.isKinematic;

        if (rb.mass < 1f) rb.mass = 100f; 
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PlayerCar") || collision.gameObject.CompareTag("AI"))
        {
            Rigidbody carRb = collision.gameObject.GetComponent<Rigidbody>();
            if (carRb == null) return;

            float impactVelocity = collision.relativeVelocity.magnitude;

            if (impactVelocity >= requiredImpactVelocity)
            {
                // 1. AKTYWACJA FIZYKI
                if (isKinematicInitially && rb.isKinematic)
                {
                    rb.isKinematic = false; 
                    // 🚨 Wymuszenie fizyki:
                    rb.useGravity = true; 
                    // Aby obiekt się przewrócił, musi mieć środek masy przesunięty
                    // lub musi być pchnięty z boku.
                }

                // 2. OBLICZENIE I ZASTOSOWANIE SIŁY
                Vector3 forceDirection = collision.contacts[0].normal;
                float calculatedForce = impactVelocity * impactForceMultiplier;
                
                // Użycie AddExplosionForce symuluje ładne pchnięcie od środka:
                rb.AddExplosionForce(calculatedForce * 0.1f, collision.contacts[0].point, 1f, 0.1f, ForceMode.Impulse);

                rb.AddForceAtPosition(
                    -forceDirection * calculatedForce, 
                    collision.contacts[0].point, 
                    ForceMode.Impulse 
                );

                // 3. ZACHOWANIE PRĘDKOŚCI SAMOCHODU
                Vector3 newCarVelocity = carRb.linearVelocity * carSpeedPreservation;
                carRb.linearVelocity = newCarVelocity; 
            }
        }
    }
}