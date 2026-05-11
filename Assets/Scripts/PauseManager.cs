using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
// This script is responsible for managing the pause state of the game.
public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public SettingsUI settingsUI;

    private bool isPaused = false;
    private bool isSettingsOpen = false;


    void Start()
    {
        // Initialize the panels.
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        AudioListener.pause = false;
        // Initialize the cursor.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        // Initialize the time scale.
        Time.timeScale = 1f;
        var systems = FindObjectsOfType<UnityEngine.EventSystems.EventSystem>();
        Debug.Log("EventSystems count: " + systems.Length);
    }

    void Update()
    {
        // Check for the pause key.
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            {
            Debug.Log("ESC detected");
            }
            // If settings is open, ESC closes settings first
            if (isSettingsOpen)
            {
                CloseSettings();
            }
            // If game is paused, ESC resumes the game
            else if (isPaused)
            {
                ResumeGame();
            }
            // Otherwise pause the game
            else
            {
                PauseGame();

            }
        }
    }
    // function to pause the game
    public void PauseGame()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);

        Time.timeScale = 0f;
        AudioListener.pause = true;

        isPaused = true;
        isSettingsOpen = false;
    }
    // function to resume the game
    public void ResumeGame()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;

        isPaused = false;
        isSettingsOpen = false;
    }
    // function to open the settings panel
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        isSettingsOpen = true;

        SettingsUI ui = settingsPanel.GetComponent<SettingsUI>();
        if (ui != null)
        {
            ui.SyncUI();
        }
        else
        {
            Debug.LogError("SettingsUI component not found on settingsPanel");
        }
    }
    // function to close the settings panel
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        isSettingsOpen = false;
    }
    // function to load the main menu
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        SceneManager.LoadScene("MainMenu");
    }
}