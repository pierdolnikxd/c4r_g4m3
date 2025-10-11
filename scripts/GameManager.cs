using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Cars")]
    public List<GameObject> allCars; // Assign all car GameObjects in Inspector

    [Header("Cameras")]
    public Camera cameraMenu; // Assign your menu camera
    public Camera cameraMain; // Assign your main gameplay camera

    [Header("Singleton")]
    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // At start: all cars and scripts/audio off, menu camera on
        SetAllCarsActive(false);
        SetAllCarScriptsAndAudio(false);
        cameraMenu.gameObject.SetActive(true);
        cameraMain.gameObject.SetActive(false);
    }

    public void SetAllCarsActive(bool active)
    {
        foreach (var car in allCars)
            if (car) car.SetActive(active);
    }

    public void SetAllCarScriptsAndAudio(bool enabled)
    {
        foreach (var car in allCars)
        {
            if (!car) continue;
            foreach (var script in car.GetComponents<MonoBehaviour>())
                script.enabled = false; // Always disable all scripts at first
            foreach (var audio in car.GetComponentsInChildren<AudioSource>())
                audio.enabled = enabled;
        }
    }

    public void SetOnlySelectedCarScriptsAndAudio(GameObject selectedCar)
    {
        foreach (var car in allCars)
        {
            bool isSelected = car == selectedCar;
            foreach (var script in car.GetComponents<MonoBehaviour>())
                script.enabled = isSelected;
            foreach (var audio in car.GetComponentsInChildren<AudioSource>())
                audio.enabled = isSelected;
        }
        
        // Ensure engine settings are reapplied for the selected car
        if (selectedCar != null)
        {
            var carController = selectedCar.GetComponent<CarController>();
            if (carController != null)
            {
                carController.ReapplyEngineSettings();
            }
        }
    }

    public void SwitchToMenuCamera()
    {
        cameraMenu.gameObject.SetActive(true);
        cameraMain.gameObject.SetActive(false);
    }

    public void SwitchToMainCamera()
    {
        cameraMenu.gameObject.SetActive(false);
        cameraMain.gameObject.SetActive(true);
    }
}
