using UnityEngine;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    private VisualElement root;

    private VisualElement mainMenu;
    private VisualElement settings;
    private VisualElement tuning;
    private VisualElement carSelection;
    
    private CarSelectionUI carSelectionUI;
    private TuningController tuningController;

    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        // wyciągamy panele
        mainMenu = root.Q<VisualElement>("MainMenu");
        settings = root.Q<VisualElement>("Settings");
        tuning = root.Q<VisualElement>("Tuning");
        carSelection = root.Q<VisualElement>("CarSelection");
        
        // Get components
        carSelectionUI = GetComponent<CarSelectionUI>();
        tuningController = GetComponent<TuningController>();

        // przypisujemy przyciski w Main Menu
        root.Q<Button>("SettingsButton").clicked += () => ShowPanel(settings);
        root.Q<Button>("CarSelectionButton").clicked += () => ShowCarSelection();
        root.Q<Button>("TuningButton").clicked += () => ShowTuning();
        root.Q<Button>("ExitButton").clicked += () => Application.Quit();

        // przyciski "Back"
        root.Q<Button>("BackFromSettings").clicked += () => ShowPanel(mainMenu);
        root.Q<Button>("BackFromTuning").clicked += () => ShowPanel(mainMenu);
        root.Q<Button>("BackFromCarSelection").clicked += () => ShowPanel(mainMenu);

        // na start pokaż Main Menu
        ShowPanel(mainMenu);
    }

    private void ShowPanel(VisualElement panelToShow)
    {
        mainMenu.style.display = DisplayStyle.None;
        settings.style.display = DisplayStyle.None;
        tuning.style.display = DisplayStyle.None;
        carSelection.style.display = DisplayStyle.None;

        panelToShow.style.display = DisplayStyle.Flex;
    }
    
    private void ShowCarSelection()
    {
        ShowPanel(carSelection);
        if (carSelectionUI != null)
        {
            carSelectionUI.ShowCarSelection();
        }
    }
    
    private void ShowTuning()
    {
        ShowPanel(tuning);
        if (tuningController != null)
        {
            tuningController.ShowTuning();
        }
    }
    
    public void ShowMainMenu()
    {
        ShowPanel(mainMenu);
        if (carSelectionUI != null)
        {
            carSelectionUI.HideCarSelection();
        }
        if (tuningController != null)
        {
            tuningController.HideTuning();
        }
    }
}