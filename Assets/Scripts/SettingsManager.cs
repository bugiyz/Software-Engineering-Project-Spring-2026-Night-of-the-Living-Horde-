using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;


public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Audio")]
    public AudioMixer mixer;

    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public float brightness = 1f;
    public Resolution[] resolutions;
    public int currentResolutionIndex;
    private Image brightnessOverlay;
    public enum Difficulty
    {
    Easy,
    Normal,
    Hard
    }
    public Difficulty difficulty = Difficulty.Easy;
    

    void Update()
{
    if (Input.GetKeyDown(KeyCode.M))
    {
        Debug.Log("Setting music to 0");
        SetMusicVolume(0.0001f);
    }

    if (Input.GetKeyDown(KeyCode.N))
    {
        Debug.Log("Setting music to full");
        SetMusicVolume(1f);
    }
}

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            resolutions = new Resolution[]
            {
                new Resolution { width = 1920, height = 1080 },
                new Resolution { width = 2560, height = 1440 },
                new Resolution { width = 3840, height = 2160 }
            };
            LoadSettings();
            ApplyAudio();
            ApplyResolution(currentResolutionIndex);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyAudio(); 

        //  find overlay in new scene
        brightnessOverlay = GameObject.Find("BrightnessOverlay")?.GetComponent<Image>();

        if (brightnessOverlay == null)
        {
            Debug.LogWarning("BrightnessOverlay not found in scene!");
            return;
        }

        ApplyBrightness();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        mixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
        SaveSettings();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        mixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        SaveSettings();
    }

    void ApplyAudio()
    {
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }

    void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    void LoadSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        brightness = PlayerPrefs.GetFloat("Brightness", 1f);
        currentResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        difficulty = (Difficulty)PlayerPrefs.GetInt("Difficulty", 0);
    }

    public void SetResolution(int index)
    {
        currentResolutionIndex = index;
        ApplyResolution(index);

        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    void ApplyResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    public void SetBrightness(float value)
    {
        // Clamp brightness so it never reaches 0
        brightness = Mathf.Clamp(value, 0.2f, 1f);

        ApplyBrightness();
        PlayerPrefs.SetFloat("Brightness", brightness);
    }

    void ApplyBrightness()
    {
        if (brightnessOverlay == null) return;

        Color c = brightnessOverlay.color;
        c.a = 1f - brightness; // invert
        brightnessOverlay.color = c;
    }
    
    public void SetDifficulty(int index)
    {
        difficulty = (Difficulty)index;
        PlayerPrefs.SetInt("Difficulty", index);
        PlayerPrefs.Save();
        Debug.Log("Difficulty set to: " + difficulty);
    }

}