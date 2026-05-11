using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// This script manages the game's settings, including audio, difficulty, and visual options.

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Audio")]
    public AudioMixer mixer;

    [Header("Music Clips")]
    public AudioSource musicSource;
    public AudioClip normalMusic;
    public AudioClip hardMusic;

    [Header("Settings Values")]
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public float brightness = 1f;
// The brightness overlay image, used to adjust the overall brightness of the screen.
    private Image brightnessOverlay;
// The difficulty level of the game.
    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }
// The current difficulty level of the game.
    public Difficulty difficulty = Difficulty.Easy;

    private void Awake()
    {
        // Ensure there is only one instance of the SettingsManager.
        if (Instance == null)
        {
            // Set the instance to this.
            Instance = this;
            // Make the instance persist across scene changes.
            DontDestroyOnLoad(gameObject);
            // Load the saved settings.
            LoadSettings();
            // Apply the loaded settings.
            ApplyAudio();
            // Apply the correct settings for whichever scene the game starts in.
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Apply the correct settings for whichever scene the game starts in.
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
        else
        {
            // If an instance already exists, destroy this one.
            Destroy(gameObject);
        }
    }
    // function to handle the destruction of the SettingsManager instance
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    // function to handle the loading of a new scene
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        ApplyAudio();

        if (scene.name == "MainLevel")
        {
            // Gameplay uses difficulty-based music.
            ApplyDifficultyMusic();
        }
        else if (scene.name == "MainMenu" || scene.name == "GameOver")
        {
            // Menu and Game Over always use normal music.
            PlayMusic(normalMusic);
        }

        brightnessOverlay = GameObject.Find("BrightnessOverlay")?.GetComponent<Image>();

        if (brightnessOverlay == null)
        {
            Debug.LogWarning("BrightnessOverlay not found in scene!");
            return;
        }

        ApplyBrightness();
    }
    // function to set the music volume
    public void SetMusicVolume(float value)
    {
        musicVolume = value;

        float safeValue = Mathf.Max(value, 0.0001f);

        if (mixer != null)
        {
            mixer.SetFloat("MusicVolume", Mathf.Log10(safeValue) * 20);
        }

        // Extra safety: if slider is all the way down, mute the music source too.
        if (musicSource != null)
        {
            musicSource.mute = value <= 0.0001f;
        }

        SaveSettings();
    }

    // function to set the SFX volume
    public void SetSFXVolume(float value)
    {
        sfxVolume = value;

        if (mixer != null)
        {
            float safeValue = Mathf.Max(value, 0.0001f);
            mixer.SetFloat("SFXVolume", Mathf.Log10(safeValue) * 20);
        }

        SaveSettings();
    }
    // function to apply the current audio settings
    private void ApplyAudio()
    {
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }
    // function to save the current settings
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("Brightness", brightness);
        PlayerPrefs.SetInt("Difficulty", (int)difficulty);
        PlayerPrefs.Save();
    }
    // function to load the saved settings
    private void LoadSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        brightness = PlayerPrefs.GetFloat("Brightness", 1f);
        difficulty = (Difficulty)PlayerPrefs.GetInt("Difficulty", 0);
    }
    // function to apply the current brightness settings
    public void SetBrightness(float value)
    {
        brightness = Mathf.Clamp(value, 0.2f, 1f);

        ApplyBrightness();
        SaveSettings();
    }
    // function to apply the current brightness settings
    private void ApplyBrightness()
    {
        if (brightnessOverlay == null)
            return;

        Color c = brightnessOverlay.color;
        c.a = 1f - brightness;
        brightnessOverlay.color = c;
    }
    // function to set the difficulty
    public void SetDifficulty(int index)
    {
        difficulty = (Difficulty)index;
        SaveSettings();

        // Do NOT call ApplyDifficultyMusic() here.
        // Hard music should only start when MainLevel loads.
        Debug.Log("Difficulty set to: " + difficulty);
    }
    // function to find the music source if it doesn't exist
    private void FindMusicSourceIfNeeded()
    {
        if (musicSource != null)
            return;

        AudioSource[] sources = FindObjectsOfType<AudioSource>();

        foreach (AudioSource source in sources)
        {
            if (source.outputAudioMixerGroup != null &&
                source.outputAudioMixerGroup.name == "Music")
            {
                musicSource = source;
                break;
            }
        }
    }
    // function to play the specified music clip
    private void PlayMusic(AudioClip clip)
    {
        FindMusicSourceIfNeeded();

        if (musicSource == null)
        {
            Debug.LogWarning("Music Source missing. Could not change music.");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning("Music clip is missing.");
            return;
        }

        if (musicSource.clip != clip)
        {
            musicSource.clip = clip;
        }

        musicSource.loop = true;

        // Respect saved volume when music starts.
        musicSource.mute = musicVolume <= 0.0001f;

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }
    /// Apply the music for the current difficulty level.
    public void ApplyDifficultyMusic()
    {
        Debug.Log("Applying music for difficulty: " + difficulty);

        AudioClip selectedClip = difficulty == Difficulty.Hard ? hardMusic : normalMusic;

        PlayMusic(selectedClip);
    }
}