using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class VisualTuningManager : MonoBehaviour
{
    [System.Serializable]
    public class VisualPart
    {
        public string partName;
        public List<GameObject> variants;
        [HideInInspector] public int currentIndex = 0;
    }

    [SerializeField] private List<VisualPart> visualParts = new List<VisualPart>();

    private VisualElement root;
    private VisualElement visualPanel;

    private Label partNameLabel;
    private Label variantLabel;
    private Button prevButton;
    private Button nextButton;
    private Button selectButton;
    private Button backButton;

    private int currentPartIndex = 0;

    private void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        partNameLabel = root.Q<Label>("PartName");
        variantLabel = root.Q<Label>("VariantName");
        prevButton = root.Q<Button>("PrevButton");
        nextButton = root.Q<Button>("NextButton");
        selectButton = root.Q<Button>("SelectButton");
        backButton = root.Q<Button>("BackVisualButton");

        prevButton.clicked += PreviousVariant;
        nextButton.clicked += NextVariant;
        selectButton.clicked += NextPart;

        LoadSavedVisuals();
        ShowCurrentPart();
    }

    private void ShowCurrentPart()
    {
        if (visualParts.Count == 0) return;

        var currentPart = visualParts[currentPartIndex];
        partNameLabel.text = currentPart.partName;
        UpdateVariantDisplay(currentPart);
    }

    private void UpdateVariantDisplay(VisualPart part)
    {
        for (int i = 0; i < part.variants.Count; i++)
            part.variants[i].SetActive(i == part.currentIndex);

        variantLabel.text = $"Variant {part.currentIndex + 1}";
    }

    private void PreviousVariant()
    {
        var part = visualParts[currentPartIndex];
        part.currentIndex = (part.currentIndex - 1 + part.variants.Count) % part.variants.Count;
        UpdateVariantDisplay(part);
        SaveVisualState(part);
    }

    private void NextVariant()
    {
        var part = visualParts[currentPartIndex];
        part.currentIndex = (part.currentIndex + 1) % part.variants.Count;
        UpdateVariantDisplay(part);
        SaveVisualState(part);
    }

    private void NextPart()
    {
        currentPartIndex = (currentPartIndex + 1) % visualParts.Count;
        ShowCurrentPart();
    }

    private void SaveVisualState(VisualPart part)
    {
        PlayerPrefs.SetInt($"Visual_{part.partName}", part.currentIndex);
        PlayerPrefs.Save();
    }

    private void LoadSavedVisuals()
    {
        foreach (var part in visualParts)
        {
            int savedIndex = PlayerPrefs.GetInt($"Visual_{part.partName}", 0);
            if (savedIndex >= 0 && savedIndex < part.variants.Count)
                part.currentIndex = savedIndex;

            UpdateVariantDisplay(part);
        }
    }

    public void ShowVisualTuning()
    {
        visualPanel.style.display = DisplayStyle.Flex;
    }
}
