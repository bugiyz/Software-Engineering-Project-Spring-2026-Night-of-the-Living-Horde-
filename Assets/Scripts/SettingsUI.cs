using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsUI : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider brightnessSlider;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown difficultyDropdown;

    private void Start()
    {
        PopulateResolutions();
        SyncUI();
    }

    public void SyncUI()
    {
        musicSlider.value = SettingsManager.Instance.musicVolume;
        sfxSlider.value = SettingsManager.Instance.sfxVolume;
        brightnessSlider.value = SettingsManager.Instance.brightness;
        if (difficultyDropdown != null)
        {
        difficultyDropdown.SetValueWithoutNotify((int)SettingsManager.Instance.difficulty);
        }
    }

    public void OnMusicChanged(float value)
    {
        SettingsManager.Instance.SetMusicVolume(value);
    }

    public void OnSFXChanged(float value)
    {
        SettingsManager.Instance.SetSFXVolume(value);
    }

    public void OnBrightnessChanged(float value)
    {
        SettingsManager.Instance.SetBrightness(value);
    }

    public void OnResolutionChanged(int index)
    {
        SettingsManager.Instance.SetResolution(index);
    }

    void PopulateResolutions()
    {
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        for (int i = 0; i < SettingsManager.Instance.resolutions.Length; i++)
        {
            Resolution res = SettingsManager.Instance.resolutions[i];
            options.Add(res.width + " x " + res.height);
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.SetValueWithoutNotify(SettingsManager.Instance.currentResolutionIndex);
        resolutionDropdown.RefreshShownValue();
    }
    
    public void OnDifficultyChanged(int index)
    {
        SettingsManager.Instance.SetDifficulty(index);
    }
}