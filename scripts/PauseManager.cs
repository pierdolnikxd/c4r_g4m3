using UnityEngine;
using UnityEngine.UIElements;
using FMOD.Studio;
using FMODUnity;

public class PauseManager : MonoBehaviour
{
    [Header("UI Toolkit")]
    public UIDocument uiDocument;
    public string mainPanelName = "MainPausePanel";
    public string[] otherPanelNames; // nazwy pozostałych paneli w UXML

    [Header("FMOD Events")]
    public EventReference[] fmodEvents; // wszystkie dźwięki, które mają pauzować
    private EventInstance[] fmodInstances;

    private VisualElement root;
    private VisualElement mainPanel;
    private VisualElement[] otherPanels;

    private bool isPaused = false;

    void Start()
{
    // UI Toolkit setup
    root = uiDocument.rootVisualElement;
    mainPanel = root.Q<VisualElement>(mainPanelName);

    otherPanels = new VisualElement[otherPanelNames.Length];
    for (int i = 0; i < otherPanelNames.Length; i++)
    {
        otherPanels[i] = root.Q<VisualElement>(otherPanelNames[i]);
    }

    // Tutaj ustawiamy, że gra startuje w menu (pauza włączona)
    isPaused = true;
    Time.timeScale = 0f;
    mainPanel.style.display = DisplayStyle.Flex;
    foreach (var panel in otherPanels)
        panel.style.display = DisplayStyle.None;

    // FMOD setup
    fmodInstances = new EventInstance[fmodEvents.Length];
    for (int i = 0; i < fmodEvents.Length; i++)
    {
        fmodInstances[i] = RuntimeManager.CreateInstance(fmodEvents[i]);
        fmodInstances[i].start();
        fmodInstances[i].setPaused(true); // od razu pauzujemy dźwięki
    }
}


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;

        // UI
        mainPanel.style.display = DisplayStyle.Flex;
        foreach (var panel in otherPanels)
            panel.style.display = DisplayStyle.None;

        // FMOD
        foreach (var instance in fmodInstances)
            instance.setPaused(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        HideAllPanels();

        // FMOD
        foreach (var instance in fmodInstances)
            instance.setPaused(false);
    }

    private void HideAllPanels()
    {
        mainPanel.style.display = DisplayStyle.None;
        foreach (var panel in otherPanels)
            panel.style.display = DisplayStyle.None;
    }

    // Przykład podpięcia do przycisku Resume w UI Toolkit
    public void OnResumeButtonClicked()
    {
        ResumeGame();
    }

    // Przykład podpięcia do przycisku Quit w UI Toolkit
    public void OnQuitButtonClicked()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
