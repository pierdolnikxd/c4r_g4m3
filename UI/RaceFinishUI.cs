using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class RaceFinishUI : MonoBehaviour
{
    public UIDocument uiDocument;

    private VisualElement root;
    private Label carNameLabel;
    private Label positionLabel;
    private Label totalTimeLabel;
    private ScrollView lapTimesScroll;
    private Button exitButton;

    private void Awake()
    {
        root = uiDocument.rootVisualElement;
        carNameLabel = root.Q<Label>("carNameLabel");
        positionLabel = root.Q<Label>("positionLabel");
        totalTimeLabel = root.Q<Label>("totalTimeLabel");
        lapTimesScroll = root.Q<ScrollView>("lapTimesScroll");
        exitButton = root.Q<Button>("exitButton");

        root.visible = false;
        exitButton.clicked += ExitRace;
    }

    public void ShowResults(string carName, int position, float totalTime, float[] lapTimes)
    {
        root.visible = true;
        Time.timeScale = 0f;

        carNameLabel.text = $"Samochód: {carName}";
        positionLabel.text = $"Pozycja: {position}";
        totalTimeLabel.text = $"Czas całkowity: {totalTime:F2}s";

        lapTimesScroll.Clear();
        for (int i = 0; i < lapTimes.Length; i++)
        {
            var label = new Label($"Okrążenie {i + 1}: {lapTimes[i]:F2}s");
            label.AddToClassList("info");
            lapTimesScroll.Add(label);
        }
    }

    private void ExitRace()
    {
        root.visible = false;
        Time.timeScale = 1f;

        if (RaceManager.Instance != null)
            RaceManager.Instance.ExitRaceMode();
    }
}
    