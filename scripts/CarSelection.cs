using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CarSelection : MonoBehaviour
{
    [System.Serializable]
    public class CarInfo
    {
        public string carName;
        public GameObject spawnedInstance;
    }

    [Header("UI Elements")]
    [SerializeField] private GameObject carSelectionPanel;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button selectButton;
    [SerializeField] private TextMeshProUGUI carNameText;
    [SerializeField] private GameObject selectedIndicator; // Visual indicator for selected car

    [Header("Cars")]
    [SerializeField] private List<CarInfo> availableCars = new List<CarInfo>();
    
    [Header("Camera")]
    // Camera movement removed; camera will stay as camera_menu from GameManager
    
    [SerializeField] private MainMenu mainMenu;
    
    public int currentCarIndex = 0;
    private bool isRotatingCar = false;
    // Camera movement variables removed

    private void Start()
{
    // Hide all cars at start
    HideAllCars();

    // Load selected car index from PlayerPrefs
    currentCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
    if (currentCarIndex >= availableCars.Count)
        currentCarIndex = 0;

    // Show only the selected car at start (if any)
    if (availableCars.Count > 0)
    {
        UpdateCarDisplay();
        ShowOnlyCurrentCar();
        UpdateSelectedIndicator();
    }

    previousButton.onClick.AddListener(PreviousCar);
    nextButton.onClick.AddListener(NextCar);
    selectButton.onClick.AddListener(SelectCar);
}

    private void HideAllCars()
    {
        foreach (var car in availableCars)
        {
            if (car.spawnedInstance != null)
            {
                car.spawnedInstance.SetActive(false);
            }
        }
    }

    public void ShowCarSelection()
    {
        carSelectionPanel.SetActive(true);
        HideAllCars();
        UpdateCarDisplay();
        ShowOnlyCurrentCar();
        // Camera movement removed
    }

    public void HideCarSelection()
    {
        carSelectionPanel.SetActive(false);
        HideAllCars();
        // Camera movement removed
    }

    private void PreviousCar()
    {
        HideAllCars();
        currentCarIndex--;
        if (currentCarIndex < 0)
            currentCarIndex = availableCars.Count - 1;
        UpdateCarDisplay();
        ShowOnlyCurrentCar();
        // Camera movement removed
    }

    private void NextCar()
    {
        HideAllCars();
        currentCarIndex++;
        if (currentCarIndex >= availableCars.Count)
            currentCarIndex = 0;
        UpdateCarDisplay();
        ShowOnlyCurrentCar();
        // Camera movement removed
    }

    private void UpdateCarDisplay()
    {
        CarInfo currentCar = availableCars[currentCarIndex];
        carNameText.text = currentCar.carName;
        UpdateSelectedIndicator();
    }

    private void UpdateSelectedIndicator()
    {
        if (selectedIndicator != null)
        {
            int selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
            selectedIndicator.SetActive(currentCarIndex == selectedCarIndex);
        }
    }

    private void SelectCar()
    {
        PlayerPrefs.SetInt("SelectedCarIndex", currentCarIndex);
        PlayerPrefs.Save();

        if (mainMenu != null)
            mainMenu.OnBackFromSettingsClick();
    }

    public void MoveCameraToSelectedCar()
    {
        var car = availableCars[currentCarIndex];
        Vector3 carPosition = car.spawnedInstance.transform.position;
        // Set your camera's target position/rotation here, or start a smooth transition
    }

    private void ShowOnlyCurrentCar()
    {
        HideAllCars();
        if (availableCars.Count > 0 && availableCars[currentCarIndex].spawnedInstance != null)
        {
            availableCars[currentCarIndex].spawnedInstance.SetActive(true);
        }
    }

    public void ShowFirstCarOnly()
    {
        if (availableCars.Count > 0)
        {
            currentCarIndex = 0;
            HideAllCars();
            availableCars[0].spawnedInstance.SetActive(true);
            UpdateCarDisplay();
        }
    }

    public void ShowSelectedCarOnly()
    {
        if (availableCars.Count > 0 && currentCarIndex >= 0 && currentCarIndex < availableCars.Count)
        {
            HideAllCars();
            availableCars[currentCarIndex].spawnedInstance.SetActive(true);
            UpdateCarDisplay();
        }
    }
} 