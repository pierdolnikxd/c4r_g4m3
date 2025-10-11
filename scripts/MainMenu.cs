using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject carSelectionPanel;
    [SerializeField] private GameObject tuningPanel;

    [Header("Menu Components")]
    [SerializeField] private CarSelection carSelection;
    [SerializeField] private Settings settings;
    [SerializeField] private TuningManager tuningManager;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button carSelectionButton;
    [SerializeField] private Button tuningButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backFromSettingsButton;
    [SerializeField] private Button backFromTuningButton;

    [Header("HUD")]
    [SerializeField] private GameObject carHud; // Reference to CarHud GameObject

    private void Start()
    {
        // Show main menu and hide other panels at start
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        if (carSelectionPanel != null)
        {
            carSelectionPanel.SetActive(false);
        }
        if (tuningPanel != null)
        {
            tuningPanel.SetActive(false);
        }

        // Hide CarHud at start
        if (carHud != null)
            carHud.SetActive(false);

        // Setup button listeners
        if (playButton != null) playButton.onClick.AddListener(OnPlayButtonClick);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsButtonClick);
        if (carSelectionButton != null) carSelectionButton.onClick.AddListener(OnCarSelectionButtonClick);
        if (tuningButton != null) tuningButton.onClick.AddListener(OnTuningButtonClick);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitButtonClick);
        if (backFromSettingsButton != null) backFromSettingsButton.onClick.AddListener(OnBackFromSettingsClick);
        if (backFromTuningButton != null) backFromTuningButton.onClick.AddListener(OnBackFromTuningClick);

        // Ensure initial state
        GameManager.Instance.SetAllCarsActive(false);
        GameManager.Instance.SetAllCarScriptsAndAudio(false);
        GameManager.Instance.SwitchToMenuCamera();
    }

    public void OnPlayButtonClick()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        carSelectionPanel.SetActive(false);
        if (tuningPanel != null) tuningPanel.SetActive(false);

        // Show CarHud when Play is clicked
        if (carHud != null)
            carHud.SetActive(true);

        // Get selected car index from CarSelection (implement this as needed)
        int selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        GameObject selectedCar = GameManager.Instance.allCars[selectedCarIndex];

        // Hide all cars, then show only the selected car
        foreach (var car in GameManager.Instance.allCars)
            if (car) car.SetActive(false);
        if (selectedCar) selectedCar.SetActive(true);

        GameManager.Instance.SetOnlySelectedCarScriptsAndAudio(selectedCar); // Only selected car is drivable/audio
        
        // Ensure engine settings are reapplied for the selected car
        var carController = selectedCar.GetComponent<CarController>();
        if (carController != null)
        {
            carController.ReapplyEngineSettings();
        }
        else
        {
            Debug.LogError($"No CarController found on selected car: {selectedCar.name}");
        }
        
        GameManager.Instance.SwitchToMainCamera();

        // Set main camera to follow the selected car
        var cameraFollow = GameManager.Instance.cameraMain.GetComponent<CameraFollow>();
        if (cameraFollow != null && selectedCar != null)
            cameraFollow.target = selectedCar.transform;
    }

    public void OnCarSelectionButtonClick()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        carSelectionPanel.SetActive(true);
        if (tuningPanel != null) tuningPanel.SetActive(false);

        GameManager.Instance.SetAllCarScriptsAndAudio(false); // No scripts/audio
        GameManager.Instance.SwitchToMenuCamera();
    }

    public void OnSettingsButtonClick()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        carSelectionPanel.SetActive(false);
        if (tuningPanel != null) tuningPanel.SetActive(false);
    }

    public void OnTuningButtonClick()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        carSelectionPanel.SetActive(false);
        if (tuningPanel != null) tuningPanel.SetActive(true);

        GameManager.Instance.SetAllCarScriptsAndAudio(false); // No scripts/audio
        GameManager.Instance.SwitchToMenuCamera();
    }

    public void OnBackFromSettingsClick()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        carSelectionPanel.SetActive(false);
        if (tuningPanel != null) tuningPanel.SetActive(false);

        // Show only the selected car in the main menu
        if (carSelection != null)
        {
            carSelection.ShowSelectedCarOnly();
        }
    }

    public void OnBackFromTuningClick()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        carSelectionPanel.SetActive(false);
        if (tuningPanel != null) tuningPanel.SetActive(false);

        // Show only the selected car in the main menu
        if (carSelection != null)
        {
            carSelection.ShowSelectedCarOnly();
        }
    }

    public void OnQuitButtonClick()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
} 