using UnityEngine;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    private VisualElement root;

    // Panele
    private VisualElement mainMenu;
    private VisualElement settings;
    private VisualElement tuningMenu;
    private VisualElement tuningEngineMenu;
    private VisualElement visualTuningMenu; // <-- DODAJ TO
    private VisualElement partSelectionPanel; // <-- DODAJ TO
    private VisualElement carSelection;

    // Komponenty logiki
    private CarSelectionUI carSelectionUI;
    private TuningController tuningController;
    private VisualTuningController visualTuningController; // <-- DODAJ TO

    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        // Panele z UXML
        mainMenu = root.Q<VisualElement>("MainMenu");
        settings = root.Q<VisualElement>("Settings");
        tuningMenu = root.Q<VisualElement>("TuningMenu");
        tuningEngineMenu = root.Q<VisualElement>("TuningEngineMenu");
        visualTuningMenu = root.Q<VisualElement>("VisualTuningMenu"); // <-- DODAJ TO
        partSelectionPanel = root.Q<VisualElement>("PartSelectionPanel"); // <-- DODAJ TO
        carSelection = root.Q<VisualElement>("CarSelection");

        // Skrypty
        carSelectionUI = GetComponent<CarSelectionUI>();
        tuningController = GetComponent<TuningController>();
        visualTuningController = GetComponent<VisualTuningController>(); // <-- DODAJ TO

        // --- MAIN MENU ---
        root.Q<Button>("SettingsButton").clicked += () => ShowPanel(settings);
        root.Q<Button>("CarSelectionButton").clicked += ShowCarSelection;
        root.Q<Button>("TuningButton").clicked += ShowTuningMenu;
        root.Q<Button>("ExitButton").clicked += Application.Quit;

        // --- SETTINGS ---
        root.Q<Button>("BackFromSettings").clicked += () => ShowPanel(mainMenu);

        // --- TUNING MENU (Engine / Visual / Back) ---
        var engineBtn = root.Q<Button>("EngineTuning");
        var visualBtn = root.Q<Button>("VisualTuning"); // <-- Poprawna nazwa z UXML
        var backFromTuningEngine = root.Q<Button>("BackFromTuning");

        if (backFromTuningEngine != null)
            backFromTuningEngine.clicked += ShowTuningMenu;

        if (engineBtn != null)
            engineBtn.clicked += ShowEngineTuning;

        if (visualBtn != null)
            visualBtn.clicked += ShowVisualTuningMenu; // <-- ZMIANA: Przekierowanie do nowej metody

        var frontBtn = root.Q<Button>("FrontBumperButton");
        if (frontBtn != null)
            frontBtn.clicked += () => 
            {
                ShowPanel(partSelectionPanel); // <-- DODAJ PRZEŁĄCZENIE PANELU
                visualTuningController?.StartPartSelection("front");
            };

        var backBtn = root.Q<Button>("BackBumperButton");
        if (backBtn != null)
            backBtn.clicked += () => 
            {
                ShowPanel(partSelectionPanel); // <-- DODAJ PRZEŁĄCZENIE PANELU
                visualTuningController?.StartPartSelection("rear");
            };

        var spoilertBtn = root.Q<Button>("SpoilerButton");
        if (spoilertBtn != null)
            spoilertBtn.clicked += () => 
            {
                ShowPanel(partSelectionPanel); // <-- DODAJ PRZEŁĄCZENIE PANELU
                visualTuningController?.StartPartSelection("spoiler");
            };

        var exhaustBtn = root.Q<Button>("ExhaustTipButton");
        if (exhaustBtn != null)
            exhaustBtn.clicked += () => 
            {
                ShowPanel(partSelectionPanel); // <-- DODAJ PRZEŁĄCZENIE PANELU
                visualTuningController?.StartPartSelection("exhaust");
            };

        var hoodBtn = root.Q<Button>("HoodButton");
        if (hoodBtn != null)
            hoodBtn.clicked += () => 
            {
                ShowPanel(partSelectionPanel); // <-- DODAJ PRZEŁĄCZENIE PANELU
                visualTuningController?.StartPartSelection("hood");
            };

        var skirtBtn = root.Q<Button>("SkirtsButton");
        if (skirtBtn != null)
            skirtBtn.clicked += () => 
            {
                ShowPanel(partSelectionPanel); // <-- DODAJ PRZEŁĄCZENIE PANELU
                visualTuningController?.StartPartSelection("skirts");
            };

    // Przycisk "Back"
    root.Q<Button>("BackFromVisualTuning").clicked += ShowTuningMenu;

    // --- PART SELECTION PANEL (Powrót) ---
    root.Q<Button>("BackFromPartSelection").clicked += ShowVisualTuningMenu;

    // --- CAR SELECTION ---
        var selectCarBtn = root.Q<Button>("SelectCarButton"); // Zakładam, że masz taki przycisk w UXML
    if (selectCarBtn != null)
        selectCarBtn.clicked += () => ShowPanel(mainMenu); // <-- DODAJ/POLEPSZ TĘ LINIĘ

        //back from tuning
        root.Q<Button>("BackFromTuningMenu").clicked += () => ShowPanel(mainMenu);

        //pokaz wybrane auto
        carSelectionUI?.ShowSelectedCar();

        // Startowy ekran
        ShowPanel(mainMenu);
    }

    // -------------------------------------------------------------------------

    public void ShowPanel(VisualElement panelToShow)
    {
        mainMenu.style.display = DisplayStyle.None;
        settings.style.display = DisplayStyle.None;
        tuningMenu.style.display = DisplayStyle.None;
        tuningEngineMenu.style.display = DisplayStyle.None;
        visualTuningMenu.style.display = DisplayStyle.None; // <-- DODAJ TO
        partSelectionPanel.style.display = DisplayStyle.None; // <-- DODAJ TO
        carSelection.style.display = DisplayStyle.None;

        panelToShow.style.display = DisplayStyle.Flex;
    }

    private void ShowCarSelection()
    {
        ShowPanel(carSelection);
        carSelectionUI?.ShowCarSelection();
    }

    public void ShowTuningMenu()
    {
        ShowPanel(tuningMenu);
    }

    public void ShowEngineTuning()
    {
        ShowPanel(tuningEngineMenu); // Możesz zmienić na osobny panel, jeśli masz.
        tuningController?.ShowTuning();
    }

    public void ShowMainMenu()
    {
        ShowPanel(mainMenu);
        carSelectionUI?.HideCarSelection();
        tuningController?.HideTuning();
    }
// Nowa metoda
public void ShowVisualTuningMenu()
{
    ShowPanel(visualTuningMenu);
    visualTuningController?.ShowVisualTuningMenu();
}
}
