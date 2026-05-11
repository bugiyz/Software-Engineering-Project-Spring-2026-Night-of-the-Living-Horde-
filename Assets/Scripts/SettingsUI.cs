using UnityEngine;
using UnityEngine.UI;
using TMPro;

// this script is responsible for managing the settings UI

public class SettingsUI : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider brightnessSlider;
    public TMP_Dropdown difficultyDropdown;

    private void Start()
    {
        SyncUI();
    }

    /// Synchronize the UI with the current settings.
    public void SyncUI()
    {
        // check if the SettingsManager instance exists
        if (SettingsManager.Instance == null)
        {
            Debug.LogWarning("SettingsManager instance not found.");
            return;
        }

        // Remove old listeners so SyncUI does not stack duplicate listeners
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveAllListeners();

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveAllListeners();

        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.RemoveAllListeners();

        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.RemoveAllListeners();

        // Set UI values without triggering events
        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(SettingsManager.Instance.musicVolume);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(SettingsManager.Instance.sfxVolume);

        if (brightnessSlider != null)
            brightnessSlider.SetValueWithoutNotify(SettingsManager.Instance.brightness);

        if (difficultyDropdown != null)
        {
            difficultyDropdown.SetValueWithoutNotify((int)SettingsManager.Instance.difficulty);

            bool inGameplay =
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainLevel";

            difficultyDropdown.interactable = !inGameplay;
        }

        // Re-add listeners
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);

        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);

        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
    }
    // function called when the music slider value changes
    public void OnMusicChanged(float value)
    {
        SettingsManager.Instance.SetMusicVolume(value);
    }
    // function called when the SFX slider value changes
    public void OnSFXChanged(float value)
    {
        SettingsManager.Instance.SetSFXVolume(value);
    }
    // function called when the brightness slider value changes
    public void OnBrightnessChanged(float value)
    {
        SettingsManager.Instance.SetBrightness(value);
    }
    // function called when the difficulty dropdown value changes
    public void OnDifficultyChanged(int index)
    {
        SettingsManager.Instance.SetDifficulty(index);
    }
}