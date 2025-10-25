using UnityEngine;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    private VisualElement root;

    // Panele
    private VisualElement mainMenu;
    private VisualElement settings;
    private VisualElement tuningMenu;
    private VisualElement carSelection;

    // Komponenty logiki
    private CarSelectionUI carSelectionUI;
    private TuningController tuningController;

    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        // Panele z UXML
        mainMenu = root.Q<VisualElement>("MainMenu");
        settings = root.Q<VisualElement>("Settings");
        tuningMenu = root.Q<VisualElement>("TuningMenu");  // nowy panel z 3 przyciskami
        carSelection = root.Q<VisualElement>("CarSelection");

        // Skrypty
        carSelectionUI = GetComponent<CarSelectionUI>();
        tuningController = GetComponent<TuningController>();

        // --- MAIN MENU ---
        root.Q<Button>("SettingsButton").clicked += () => ShowPanel(settings);
        root.Q<Button>("CarSelectionButton").clicked += ShowCarSelection;
        root.Q<Button>("TuningButton").clicked += ShowTuningMenu;
        root.Q<Button>("ExitButton").clicked += Application.Quit;

        // --- SETTINGS ---
        root.Q<Button>("BackFromSettings").clicked += () => ShowPanel(mainMenu);

        // --- TUNING MENU (Engine / Visual / Back) ---
        var engineBtn = root.Q<Button>("EngineButton");
        var visualBtn = root.Q<Button>("VisualButton");
        var backBtn = root.Q<Button>("BackFromTuningMenu");

        if (engineBtn != null)
            engineBtn.clicked += ShowEngineTuning;

        if (visualBtn != null)
            visualBtn.clicked += () => Debug.Log("Visual tuning - placeholder");

        if (backBtn != null)
            backBtn.clicked += () => ShowPanel(mainMenu);

        // --- CAR SELECTION ---
        root.Q<Button>("BackFromCarSelection").clicked += () => ShowPanel(mainMenu);

        // Startowy ekran
        ShowPanel(mainMenu);
    }

    // -------------------------------------------------------------------------

    private void ShowPanel(VisualElement panelToShow)
    {
        mainMenu.style.display = DisplayStyle.None;
        settings.style.display = DisplayStyle.None;
        tuningMenu.style.display = DisplayStyle.None;
        carSelection.style.display = DisplayStyle.None;

        panelToShow.style.display = DisplayStyle.Flex;
    }

    private void ShowCarSelection()
    {
        ShowPanel(carSelection);
        carSelectionUI?.ShowCarSelection();
    }

    private void ShowTuningMenu()
    {
        ShowPanel(tuningMenu);
    }

    private void ShowEngineTuning()
    {
        ShowPanel(tuningMenu); // Możesz zmienić na osobny panel, jeśli masz.
        tuningController?.ShowTuning();
    }

    public void ShowMainMenu()
    {
        ShowPanel(mainMenu);
        carSelectionUI?.HideCarSelection();
        tuningController?.HideTuning();
    }
}
