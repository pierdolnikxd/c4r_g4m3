using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Settings : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;

    [Header("Game Settings")]
    [SerializeField] private Toggle turboToggle;

    private void Start()
    {
        // Initialize volume slider
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
            SetVolume(volumeSlider.value);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        // Initialize turbo toggle
        if (turboToggle != null)
        {
            turboToggle.isOn = PlayerPrefs.GetInt("TurboEnabled", 1) == 1;
            SetTurbo(turboToggle.isOn);
            turboToggle.onValueChanged.AddListener(SetTurbo);
        }
    }

    public void SetVolume(float volume)
    {
        // Convert volume to decibels (avoid log10 of zero)
        float dbVolume = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat("MasterVolume", dbVolume);
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.Save();
    }

    public void SetTurbo(bool enabled)
    {
        PlayerPrefs.SetInt("TurboEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }
} 