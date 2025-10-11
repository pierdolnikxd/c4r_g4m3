using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TuningManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI turboNameText;
    [SerializeField] private Toggle turboToggle;
    [SerializeField] private Button backButton;

    [Header("Turbo Settings")]
    [SerializeField] private string turboName = "Garrett GT2860RS";
    [SerializeField] private float maxPSI = 20f;

    private void Start()
    {
        // Setup UI
        if (turboNameText != null)
            turboNameText.text = turboName;

        // Load turbo enabled state from PlayerPrefs
        bool turboEnabled = PlayerPrefs.GetInt("TurboEnabled", 1) == 1; // Default to enabled
        if (turboToggle != null)
        {
            turboToggle.isOn = turboEnabled;
            turboToggle.onValueChanged.AddListener(OnTurboToggleChanged);
        }

        // Setup back button
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClick);
        }
    }

    private void OnTurboToggleChanged(bool isEnabled)
    {
        // Save turbo enabled state
        PlayerPrefs.SetInt("TurboEnabled", isEnabled ? 1 : 0);
        PlayerPrefs.Save();

        // Apply turbo settings to selected car
        ApplyTurboSettingsToSelectedCar(isEnabled);
    }

    private void ApplyTurboSettingsToSelectedCar(bool turboEnabled)
    {
        // Get selected car index
        int selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        
        // Get the selected car from GameManager
        if (GameManager.Instance != null && GameManager.Instance.allCars != null && 
            selectedCarIndex < GameManager.Instance.allCars.Count)
        {
            GameObject selectedCar = GameManager.Instance.allCars[selectedCarIndex];
            if (selectedCar != null)
            {
                // Enable/disable TurboSystem component
                TurboSystem turboSystem = selectedCar.GetComponent<TurboSystem>();
                if (turboSystem != null)
                {
                    turboSystem.enabled = turboEnabled;
                    
                    // If turbo is disabled, reset PSI to 0
                    if (!turboEnabled)
                    {
                        // We can't directly access private currentPSI, but the system will handle it
                        Debug.Log($"Turbo system {(turboEnabled ? "enabled" : "disabled")} for {selectedCar.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"No TurboSystem found on {selectedCar.name}");
                }
            }
        }
    }

    private void OnBackButtonClick()
    {
        // This will be handled by MainMenu's back button
        // The TuningManager just manages the UI state
    }

    // Public method to get current turbo state
    public bool IsTurboEnabled()
    {
        return PlayerPrefs.GetInt("TurboEnabled", 1) == 1;
    }

    // Public method to get turbo name
    public string GetTurboName()
    {
        return turboName;
    }

    // Public method to get max PSI
    public float GetMaxPSI()
    {
        return maxPSI;
    }
}
