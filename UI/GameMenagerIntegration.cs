using UnityEngine;
using UnityEngine.UIElements;

public class GameManagerIntegration : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarSelectionUI carSelectionUI;
    [SerializeField] private MenuManager menuManager;
    
    void Start()
    {
        // Get components if not assigned
        if (carSelectionUI == null)
            carSelectionUI = GetComponent<CarSelectionUI>();
        if (menuManager == null)
            menuManager = GetComponent<MenuManager>();
            
        // Setup play button functionality
        SetupPlayButton();
    }
    
    private void SetupPlayButton()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var playButton = root.Q<Button>("PlayButton");
        
        if (playButton != null)
        {
            playButton.clicked += OnPlayButtonClick;
        }
    }
    
    private void OnPlayButtonClick()
    {
        // Hide all UI panels
        if (menuManager != null)
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var mainMenu = root.Q<VisualElement>("MainMenu");
            var settings = root.Q<VisualElement>("Settings");
            var tuning = root.Q<VisualElement>("Tuning");
            var carSelection = root.Q<VisualElement>("CarSelection");
            
            mainMenu.style.display = DisplayStyle.None;
            settings.style.display = DisplayStyle.None;
            tuning.style.display = DisplayStyle.None;
            carSelection.style.display = DisplayStyle.None;
            Time.timeScale = 1f;
        }
        
        // Get selected car from CarSelectionUI
        if (carSelectionUI != null)
        {
            var selectedCar = carSelectionUI.GetSelectedCar();
            if (selectedCar != null)
            {
                // Use the existing GameManager system
                if (GameManager.Instance != null)
                {
                    // Find the car in GameManager's allCars list
                    int selectedCarIndex = FindCarIndexInGameManager(selectedCar.spawnedInstance);
                    
                    if (selectedCarIndex >= 0)
                    {
                        // Set the selected car in PlayerPrefs
                        PlayerPrefs.SetInt("SelectedCarIndex", selectedCarIndex);
                        PlayerPrefs.Save();
                        
                        // Get the car from GameManager
                        GameObject gameManagerCar = GameManager.Instance.allCars[selectedCarIndex];
                        
                        // Hide all cars, then show only the selected car
                        foreach (var car in GameManager.Instance.allCars)
                            if (car) car.SetActive(false);
                        if (gameManagerCar) gameManagerCar.SetActive(true);
                        
                        // Enable scripts and audio for selected car only
                        GameManager.Instance.SetOnlySelectedCarScriptsAndAudio(gameManagerCar);
                        
                        // Ensure engine settings are reapplied for the selected car
                        var carController = gameManagerCar.GetComponent<CarController>();
                        if (carController != null)
                        {
                            carController.ReapplyEngineSettings();
                        }
                        
                        // Switch to main camera
                        GameManager.Instance.SwitchToMainCamera();
                        
                        // Set main camera to follow the selected car
                        var cameraFollow = GameManager.Instance.cameraMain.GetComponent<CameraFollow>();
                        if (cameraFollow != null && gameManagerCar != null)
                            cameraFollow.target = gameManagerCar.transform;
                            
                        Debug.Log($"Starting game with car: {selectedCar.displayName}");
                    }
                    else
                    {
                        Debug.LogError("Selected car not found in GameManager's allCars list");
                    }
                }
                else
                {
                    Debug.LogError("GameManager.Instance is null");
                }
            }
            else
            {
                Debug.LogError("No car selected");
            }
        }
    }
    
    private int FindCarIndexInGameManager(GameObject targetCar)
    {
        if (GameManager.Instance == null || GameManager.Instance.allCars == null)
            return -1;
            
        for (int i = 0; i < GameManager.Instance.allCars.Count; i++)
        {
            if (GameManager.Instance.allCars[i] == targetCar)
            {
                return i;
            }
        }
        return -1;
    }
}