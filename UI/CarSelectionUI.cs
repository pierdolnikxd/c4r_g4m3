using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class CarSelectionUI : MonoBehaviour
{
    [System.Serializable]
    public class CarInfo
    {
        public string carName;
        public string displayName;
        public GameObject spawnedInstance;
    }

    [Header("Cars")]
    [SerializeField] private List<CarInfo> availableCars = new List<CarInfo>();
    
    [Header("References")]
    [SerializeField] private MenuManager menuManager;
    
    private VisualElement root;
    private VisualElement carSelectionPanel;
    private Label carNameLabel;
    private VisualElement selectedIndicator;
    private Button previousButton;
    private Button nextButton;
    private Button selectButton;
    
    private int currentCarIndex = 0;
    
    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        
        // Get UI elements
        carSelectionPanel = root.Q<VisualElement>("CarSelection");
        carNameLabel = root.Q<Label>("CarNameLabel");
        selectedIndicator = root.Q<VisualElement>("SelectedIndicator");
        previousButton = root.Q<Button>("PreviousCarButton");
        nextButton = root.Q<Button>("NextCarButton");
        selectButton = root.Q<Button>("SelectCarButton");
        
        // Setup car data if not already set
        if (availableCars.Count == 0)
        {
            SetupDefaultCars();
        }
        
        // Load selected car index from PlayerPrefs
        currentCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        if (currentCarIndex >= availableCars.Count)
            currentCarIndex = 0;
        
        // Setup button events
        previousButton.clicked += PreviousCar;
        nextButton.clicked += NextCar;
        selectButton.clicked += SelectCar;
        
        // Initialize display
        UpdateCarDisplay();
        ShowOnlyCurrentCar();
        UpdateSelectedIndicator();
    }
    
    private void SetupDefaultCars()
    {
        // Find cars in the scene by name
        GameObject nissanCar = GameObject.Find("r34");
        GameObject mazdaCar = GameObject.Find("mazda_rx7-fc");
        
        if (nissanCar != null)
        {
            availableCars.Add(new CarInfo
            {
                carName = "nissan",
                displayName = "Nissan R34",
                spawnedInstance = nissanCar
            });
        }
        
        if (mazdaCar != null)
        {
            availableCars.Add(new CarInfo
            {
                carName = "mazda",
                displayName = "Mazda RX7-FC",
                spawnedInstance = mazdaCar
            });
        }
        
        Debug.Log($"CarSelectionUI: Found {availableCars.Count} cars");
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
    
    private void PreviousCar()
    {
        HideAllCars();
        currentCarIndex--;
        if (currentCarIndex < 0)
            currentCarIndex = availableCars.Count - 1;
        UpdateCarDisplay();
        ShowOnlyCurrentCar();
    }
    
    private void NextCar()
    {
        HideAllCars();
        currentCarIndex++;
        if (currentCarIndex >= availableCars.Count)
            currentCarIndex = 0;
        UpdateCarDisplay();
        ShowOnlyCurrentCar();
    }
    
    private void UpdateCarDisplay()
    {
        if (availableCars.Count > 0 && currentCarIndex < availableCars.Count)
        {
            CarInfo currentCar = availableCars[currentCarIndex];
            carNameLabel.text = currentCar.displayName;
            UpdateSelectedIndicator();
        }
    }
    
    private void UpdateSelectedIndicator()
    {
        if (selectedIndicator != null)
        {
            int selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
            selectedIndicator.style.display = (currentCarIndex == selectedCarIndex) ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
    
    private void SelectCar()
    {
        PlayerPrefs.SetInt("SelectedCarIndex", currentCarIndex);
        PlayerPrefs.Save();
        
        Debug.Log($"Selected car: {availableCars[currentCarIndex].displayName}");
        
        // Go back to main menu
        if (menuManager != null)
        {
            menuManager.ShowMainMenu();
        }
    }
    
    private void ShowOnlyCurrentCar()
    {
        HideAllCars();
        if (availableCars.Count > 0 && currentCarIndex < availableCars.Count && availableCars[currentCarIndex].spawnedInstance != null)
        {
            availableCars[currentCarIndex].spawnedInstance.SetActive(true);
        }
    }
    
    public void ShowCarSelection()
    {
        // This method can be called when the car selection panel is shown
        HideAllCars();
        UpdateCarDisplay();
        ShowOnlyCurrentCar();
    }
    
    public void HideCarSelection()
    {
        HideAllCars();
    }
    
    // Public method to get the currently selected car
    public CarInfo GetSelectedCar()
    {
        if (availableCars.Count > 0 && currentCarIndex < availableCars.Count)
        {
            return availableCars[currentCarIndex];
        }
        return null;
    }
    
    // Public method to get all available cars
    public List<CarInfo> GetAllCars()
    {
        return availableCars;
    }
}