using UnityEngine;
using UnityEngine.UIElements;

public class TuningController : MonoBehaviour
{
    [System.Serializable]
    public class TurboStage
    {
        public string name;
        public float maxPSI;
        public string description;
    }

    [System.Serializable]
    public class ECUStage
    {
        public string name;
        public float powerMultiplier;
        public string description;
    }

    [Header("Turbo Stages")]
    [SerializeField] private TurboStage[] turboStages = new TurboStage[]
    {
        new TurboStage { name = "None", maxPSI = 0f, description = "No turbo system" },
        new TurboStage { name = "Stage 1", maxPSI = 5f, description = "Light boost - 5 PSI" },
        new TurboStage { name = "Stage 2", maxPSI = 10f, description = "Medium boost - 10 PSI" },
        new TurboStage { name = "Stage 3", maxPSI = 15f, description = "High boost - 15 PSI" }
    };

    [Header("ECU Stages")]
    [SerializeField] private ECUStage[] ecuStages = new ECUStage[]
    {
        new ECUStage { name = "Stock", powerMultiplier = 1f, description = "Stock engine mapping" },
        new ECUStage { name = "Stage 1", powerMultiplier = 1.1f, description = "+10% power" },
        new ECUStage { name = "Stage 2", powerMultiplier = 1.2f, description = "+20% power" },
        new ECUStage { name = "Stage 3", powerMultiplier = 1.3f, description = "+30% power" }
    };

    private VisualElement root;
    private VisualElement tuningPanel;

    // Kategorie
    private Button ecuButton;
    private Button turboButton;
    private Button exhaustButton;
    private Button fuelButton;

    // Sekcje
    private VisualElement ecuSection;
    private VisualElement turboSection;
    private VisualElement exhaustSection;
    private VisualElement fuelSection;

    // Dynamiczne kontenery
    private VisualElement turboStageContainer;
    private VisualElement ecuStageContainer;

    // Statusy
    private Label turboStatusLabel;
    private Label ecuStatusLabel;

    private int currentTurboStage = 0;
    private int currentECUStage = 0;

    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        tuningPanel = root.Q<VisualElement>("Tuning");

        // Kategorie
        ecuButton = root.Q<Button>("ECUButton");
        turboButton = root.Q<Button>("TurboButton");
        exhaustButton = root.Q<Button>("ExhaustButton");
        fuelButton = root.Q<Button>("FuelButton");

        // Sekcje
        ecuSection = root.Q<VisualElement>("ECUSection");
        turboSection = root.Q<VisualElement>("TurboSection");
        exhaustSection = root.Q<VisualElement>("ExhaustSection");
        fuelSection = root.Q<VisualElement>("FuelSection");

        // Kontenery dynamiczne
        turboStageContainer = root.Q<VisualElement>("TurboStageContainer");
        ecuStageContainer = root.Q<VisualElement>("ECUStageContainer");

        turboStatusLabel = root.Q<Label>("TurboStatusLabel");
        ecuStatusLabel = root.Q<Label>("ECUStatusLabel");

        SetupCategoryButtons();
        BuildTurboUI();
        BuildECUUI();

        LoadTuningSettings();
    }

    private void SetupCategoryButtons()
    {
        ecuButton.clicked += () => ShowTuningSection("ECU");
        turboButton.clicked += () => ShowTuningSection("Turbo");
        exhaustButton.clicked += () => ShowTuningSection("Exhaust");
        fuelButton.clicked += () => ShowTuningSection("Fuel");
    }

    // -----------------------------
    // DYNAMICZNE TWORZENIE UI
    // -----------------------------
    private void BuildTurboUI()
    {
        turboStageContainer.Clear();

        for (int i = 0; i < turboStages.Length; i++)
        {
            int index = i;
            TurboStage stage = turboStages[i];

            var button = new Button();
            button.AddToClassList("turbo-option-button");
            button.text = $"{stage.name} ({stage.maxPSI} PSI)";
            button.tooltip = stage.description;
            button.clicked += () => SetTurboStage(index);

            turboStageContainer.Add(button);
        }
    }

    private void BuildECUUI()
    {
        ecuStageContainer.Clear();

        for (int i = 0; i < ecuStages.Length; i++)
        {
            int index = i;
            ECUStage stage = ecuStages[i];

            var button = new Button();
            button.AddToClassList("turbo-option-button");
            button.text = $"{stage.name} ({(stage.powerMultiplier - 1f) * 100f:+0;-0}% power)";
            button.tooltip = stage.description;
            button.clicked += () => SetECUStage(index);

            ecuStageContainer.Add(button);
        }
    }

    // -----------------------------
    // SEKCJE UI
    // -----------------------------
    private void ShowTuningSection(string sectionName)
    {
        ecuSection.style.display = DisplayStyle.None;
        turboSection.style.display = DisplayStyle.None;
        exhaustSection.style.display = DisplayStyle.None;
        fuelSection.style.display = DisplayStyle.None;

        ecuButton.RemoveFromClassList("selected");
        turboButton.RemoveFromClassList("selected");
        exhaustButton.RemoveFromClassList("selected");
        fuelButton.RemoveFromClassList("selected");

        switch (sectionName)
        {
            case "ECU":
                ecuSection.style.display = DisplayStyle.Flex;
                ecuButton.AddToClassList("selected");
                break;
            case "Turbo":
                turboSection.style.display = DisplayStyle.Flex;
                turboButton.AddToClassList("selected");
                break;
            case "Exhaust":
                exhaustSection.style.display = DisplayStyle.Flex;
                exhaustButton.AddToClassList("selected");
                break;
            case "Fuel":
                fuelSection.style.display = DisplayStyle.Flex;
                fuelButton.AddToClassList("selected");
                break;
        }
    }

    // -----------------------------
    // TURBO
    // -----------------------------
    private void SetTurboStage(int stage)
    {
        currentTurboStage = stage;

        foreach (var child in turboStageContainer.Children())
            child.RemoveFromClassList("selected");

        turboStageContainer.ElementAt(stage).AddToClassList("selected");

        turboStatusLabel.text = $"Current: {turboStages[stage].name} ({turboStages[stage].maxPSI} PSI)";
        PlayerPrefs.SetInt("TurboStage", stage);
        PlayerPrefs.SetFloat("TurboMaxPSI", turboStages[stage].maxPSI);
        PlayerPrefs.Save();

        ApplyTurboToSelectedCar();
    }

    private void ApplyTurboToSelectedCar()
    {
        int selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        if (GameManager.Instance == null || GameManager.Instance.allCars == null) return;
        if (selectedCarIndex >= GameManager.Instance.allCars.Count) return;

        GameObject selectedCar = GameManager.Instance.allCars[selectedCarIndex];
        if (selectedCar == null) return;

        TurboSystem turboSystem = selectedCar.GetComponent<TurboSystem>();
        if (turboSystem != null)
        {
            turboSystem.maxPSI = turboStages[currentTurboStage].maxPSI;
            turboSystem.enabled = currentTurboStage > 0;
        }
    }

    // -----------------------------
    // ECU
    // -----------------------------
    private void SetECUStage(int stage)
    {
        currentECUStage = stage;

        foreach (var child in ecuStageContainer.Children())
            child.RemoveFromClassList("selected");

        ecuStageContainer.ElementAt(stage).AddToClassList("selected");

        ecuStatusLabel.text = $"Current: {ecuStages[stage].name}";
        PlayerPrefs.SetInt("ECUStage", stage);
        PlayerPrefs.Save();

        ApplyECUToSelectedCar();
    }

    private void ApplyECUToSelectedCar()
    {
        int selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        if (GameManager.Instance == null || GameManager.Instance.allCars == null) return;
        if (selectedCarIndex >= GameManager.Instance.allCars.Count) return;

        GameObject selectedCar = GameManager.Instance.allCars[selectedCarIndex];
        if (selectedCar == null) return;

        CarController carController = selectedCar.GetComponent<CarController>();
        if (carController == null) return;

        BaseEngine engine = carController.selectedEngine;
        if (engine == null || engine.baseEngineTorqueCurve == null) return;

        float multiplier = ecuStages[currentECUStage].powerMultiplier;

        if (multiplier == 1f)
        {
            engine.engineTorqueCurve = new AnimationCurve(engine.baseEngineTorqueCurve.keys);
        }
        else
        {
            AnimationCurve newCurve = new AnimationCurve(engine.baseEngineTorqueCurve.keys);
            for (int i = 0; i < newCurve.keys.Length; i++)
            {
                Keyframe k = newCurve.keys[i];
                k.value *= multiplier;
                newCurve.MoveKey(i, k);
            }
            engine.engineTorqueCurve = newCurve;
        }

        Debug.Log($"Applied ECU stage {ecuStages[currentECUStage].name} to engine {engine.name} (x{multiplier})");
    }

    // -----------------------------
    // ZAPIS I WYŚWIETLANIE
    // -----------------------------
    private void LoadTuningSettings()
    {
        currentTurboStage = PlayerPrefs.GetInt("TurboStage", 0);
        if (currentTurboStage >= turboStages.Length) currentTurboStage = 0;
        SetTurboStage(currentTurboStage);

        currentECUStage = PlayerPrefs.GetInt("ECUStage", 0);
        if (currentECUStage >= ecuStages.Length) currentECUStage = 0;
        SetECUStage(currentECUStage);
    }

    public void ShowTuning()
    {
        LoadTuningSettings();
        tuningPanel.style.display = DisplayStyle.Flex;
    }

    public void HideTuning()
    {
        tuningPanel.style.display = DisplayStyle.None;
    }
}