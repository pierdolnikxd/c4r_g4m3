using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EngineSoundTesterHUD : MonoBehaviour
{
    public Dropdown engineDropdown;
    public Slider rpmSlider;
    public Text rpmValueText;

    public List<BaseEngine> enginePrefabs; // Przypisz w Inspectorze wszystkie silniki
    private BaseEngine currentEngine;

    void Start()
    {
        // Wypełnij dropdown nazwami silników
        engineDropdown.ClearOptions();
        List<string> names = new List<string>();
        foreach (var engine in enginePrefabs)
            names.Add(engine.name);
        engineDropdown.AddOptions(names);

        engineDropdown.onValueChanged.AddListener(OnEngineChanged);
        rpmSlider.onValueChanged.AddListener(OnRPMChanged);

        // Ustaw pierwszy silnik jako domyślny
        if (enginePrefabs.Count > 0)
            SetEngine(0);
    }

    void OnEngineChanged(int index)
    {
        SetEngine(index);
    }

    void SetEngine(int index)
    {
        if (currentEngine != null)
            currentEngine.gameObject.SetActive(true);

        currentEngine = enginePrefabs[index];
        currentEngine.gameObject.SetActive(true);

        // Ustaw zakres suwaka na RPM silnika
        rpmSlider.minValue = currentEngine.idleRPM;
        rpmSlider.maxValue = currentEngine.maxEngineRPM;
        rpmSlider.value = currentEngine.idleRPM;
        UpdateRPM();
    }

    void OnRPMChanged(float value)
    {
        UpdateRPM();
    }

    void UpdateRPM()
    {
        if (currentEngine != null)
        {
            // Wymuś RPM na silniku (musisz mieć publiczną/protected metodę do testu!)
            currentEngine.TestSetRPM(rpmSlider.value);
            rpmValueText.text = $"RPM: {Mathf.RoundToInt(rpmSlider.value)}";
        }
    }
}