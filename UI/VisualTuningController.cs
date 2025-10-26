using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class VisualTuningController : MonoBehaviour
{
    private VisualElement root;
    private MenuManager menuManager;
    private CarSelectionUI carSelectionUI; // Będzie potrzebny do pobrania instancji samochodu

    // Panele
    private VisualElement visualTuningMenu;
    private VisualElement partSelectionPanel;

    // Elementy UI
    private Label partNameLabel;
    private Button prevButton;
    private Button nextButton;
    private Button selectButton;

    // Bieżący stan
    private string currentPartCategory; // np. "Front", "Rear", "Spoiler"
    private int currentIndex = 0;
    
    // Słownik przechowujący obiekty dla każdej kategorii (partCategory -> Lista GameObjects)
    private Dictionary<string, List<GameObject>> carParts = new Dictionary<string, List<GameObject>>();
    
    // Wymagane nazwy GameObjects na samochodzie
    private readonly string[] partCategories = { "front", "rear", "spoiler", "exhaust", "hood", "skirts" };

    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        menuManager = GetComponent<MenuManager>();
        carSelectionUI = GetComponent<CarSelectionUI>();

        // 1. Zlokalizuj panele (zakładając, że je dodasz w UXML, patrz sekcja 2)
        visualTuningMenu = root.Q<VisualElement>("VisualTuningMenu");
        partSelectionPanel = root.Q<VisualElement>("PartSelectionPanel");

        // 2. Zlokalizuj elementy kontrolne
        partNameLabel = root.Q<Label>("PartNameLabel");
        prevButton = root.Q<Button>("PrevPartButton");
        nextButton = root.Q<Button>("NextPartButton");
        selectButton = root.Q<Button>("SelectPartButton");
        
        // 3. Przypisz akcje do przycisków
        if (prevButton != null) prevButton.clicked += () => ChangePart(-1);
        if (nextButton != null) nextButton.clicked += () => ChangePart(1);
        if (selectButton != null) selectButton.clicked += SaveCurrentPart;
    }

    // W VisualTuningController.cs
// Ta metoda jest wywoływana przez MenuManager po kliknięciu przycisku "Visual"
public void ShowVisualTuningMenu()
{
    // WAŻNE: Inicjalizuj części tutaj, gdy tylko wejdziesz do menu Visual Tuning.
    // Zapewni to, że carParts jest wypełnione przed kliknięciem kategorii.
    if (!InitializeCarParts()) 
    {
        Debug.LogWarning("Brak aktywnego samochodu lub części do tuningu wizualnego. Powrót do Tuning Menu.");
        menuManager.ShowTuningMenu(); // Wracamy do głównego menu tuningu
        return;
    }
    
    // Opcjonalnie: Zresetuj stan, aby uniknąć przypadkowych kliknięć strzałek
    currentPartCategory = null; 
}

    // Wywoływane z MenuManager po kliknięciu kategorii (np. "Front Bumper")
    // W VisualTuningController.cs
public void StartPartSelection(string partCategory)
{
    // Ustawienie klucza kategorii
    currentPartCategory = partCategory; // <--- To jest klucz, który był null

    // 1. Sprawdzenie, czy dla tej kategorii w ogóle są jakieś części
    if (!carParts.ContainsKey(currentPartCategory) || carParts[currentPartCategory].Count == 0)
    {
        Debug.LogWarning($"Brak części w kategorii: {partCategory} na samochodzie. Powrót do Visual Tuning Menu.");
        
        // Zapewnij, że panel wyboru części się nie włączy, jeśli nie ma części
        menuManager.ShowVisualTuningMenu(); // Wracamy do menu kategorii
        return;
    }

    // 2. Jeśli mamy części, ładujemy indeks i aktualizujemy UI
    LoadCurrentPartIndex(); 
    UpdatePartVisibility();
    UpdatePartSelectionUI();
    
    // Panel jest przełączany już przez MenuManager, więc tu nie musimy
}

    private bool InitializeCarParts()
    {
        carParts.Clear();
        GameObject selectedCarInstance = carSelectionUI?.GetSelectedCar()?.spawnedInstance;

        if (selectedCarInstance == null) return false;

        foreach (var category in partCategories)
        {
            // Znajdź główny GameObject kategorii (np. "Front") na samochodzie
            Transform categoryParent = selectedCarInstance.transform.Find(category);
            
            if (categoryParent != null)
            {
                // Zbierz wszystkie dzieci (poszczególne części/zderzaki)
                List<GameObject> parts = new List<GameObject>();
                for (int i = 0; i < categoryParent.childCount; i++)
                {
                    parts.Add(categoryParent.GetChild(i).gameObject);
                }
                carParts.Add(category, parts);
            }
        }

        ApplySavedVisualTuning(selectedCarInstance); // Zastosuj zapisane ustawienia po inicjalizacji
        return carParts.Count > 0;
    }

    private void ChangePart(int direction)
    {
        // FIX BŁĘDU: Sprawdzenie, czy kategoria jest ustawiona
        if (string.IsNullOrEmpty(currentPartCategory)) 
        {
             Debug.LogError("Error: Brak wybranej kategorii części.");
             return; 
        }

        if (!carParts.ContainsKey(currentPartCategory) || carParts[currentPartCategory].Count == 0) return;

        int maxIndex = carParts[currentPartCategory].Count - 1;
        currentIndex += direction;

        // Zapętlenie
        if (currentIndex > maxIndex) currentIndex = 0;
        if (currentIndex < 0) currentIndex = maxIndex;

        UpdatePartVisibility();
        UpdatePartSelectionUI();
    }

    private void UpdatePartVisibility()
    {
        if (!carParts.ContainsKey(currentPartCategory)) return;

        List<GameObject> parts = carParts[currentPartCategory];

        // Wyłącz wszystkie części w bieżącej kategorii
        foreach (var part in parts)
        {
            part.SetActive(false);
        }

        // Włącz tylko wybraną część
        if (currentIndex >= 0 && currentIndex < parts.Count)
        {
            parts[currentIndex].SetActive(true);
        }
    }

    private void UpdatePartSelectionUI()
    {
        if (!carParts.ContainsKey(currentPartCategory)) return;
        List<GameObject> parts = carParts[currentPartCategory];

        string partName = parts[currentIndex].name;
        partNameLabel.text = $"{currentPartCategory}: {partName} ({currentIndex + 1}/{parts.Count})";
        
        // Przycisk "Select" będzie widoczny
        selectButton.style.display = DisplayStyle.Flex;
    }
    
    private void SaveCurrentPart()
    {
        // Klucz zapisu: np. "Car_B01_Front_Index"
        string saveKey = GetPartSaveKey(currentPartCategory);
        PlayerPrefs.SetInt(saveKey, currentIndex);
        PlayerPrefs.Save();
        
        Debug.Log($"Zapisano {currentPartCategory} na index: {currentIndex}");
        
        // Opcjonalnie: Zmiana tekstu przycisku Select na np. "Selected" na chwilę
        selectButton.text = "Selected!";
        Invoke("ResetSelectButtonText", 0.5f);
    }
    
    private void ResetSelectButtonText()
    {
        selectButton.text = "Select";
    }

    private void LoadCurrentPartIndex()
    {
        string saveKey = GetPartSaveKey(currentPartCategory);
        currentIndex = PlayerPrefs.GetInt(saveKey, 0); // Domyślnie na 0
        
        // Zabezpieczenie przed nieistniejącym indeksem
        if (carParts.ContainsKey(currentPartCategory) && currentIndex >= carParts[currentPartCategory].Count)
        {
            currentIndex = 0;
        }
    }

    private string GetPartSaveKey(string partCategory)
    {
        // Użyj unikalnego ID samochodu, jeśli masz (np. z CarSelectionUI).
        // Na razie użyjemy nazwy GameObjecktu samochodu + kategorii
        string carName = carSelectionUI?.GetSelectedCar()?.spawnedInstance?.name ?? "DefaultCar";
        return $"{carName}_{partCategory}_Index";
    }
    
    // Ta metoda powinna być wywołana raz po załadowaniu samochodu (np. w InitializeCarParts)
    public void ApplySavedVisualTuning(GameObject carInstance)
    {
        if (carInstance == null) return;

        foreach (var category in partCategories)
        {
            Transform categoryParent = carInstance.transform.Find(category);
            if (categoryParent == null) continue;

            List<GameObject> parts = new List<GameObject>();
            for (int i = 0; i < categoryParent.childCount; i++)
            {
                parts.Add(categoryParent.GetChild(i).gameObject);
            }

            if (parts.Count == 0) continue;

            string saveKey = GetPartSaveKey(category);
            int savedIndex = PlayerPrefs.GetInt(saveKey, 0);

            // Zabezpieczenie
            if (savedIndex >= parts.Count) savedIndex = 0;
            
            // Ustaw widoczność zgodnie z zapisanym indeksem
            for (int i = 0; i < parts.Count; i++)
            {
                parts[i].SetActive(i == savedIndex);
            }
        }
    }
}