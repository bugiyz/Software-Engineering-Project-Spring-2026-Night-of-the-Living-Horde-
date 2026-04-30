using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public SettingsUI settingsUI;

    private bool isPaused = false;
    private bool isSettingsOpen = false;


    void Start()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        var systems = FindObjectsOfType<UnityEngine.EventSystems.EventSystem>();
        Debug.Log("EventSystems count: " + systems.Length);
    }

    void Update()
    {
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

    public void PauseGame()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;
        isSettingsOpen = false;
    }

    public void ResumeGame()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
        isSettingsOpen = false;
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        isSettingsOpen = true;
        SettingsUI ui = settingsPanel.GetComponent<SettingsUI>();
        ui.SyncUI();
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        isSettingsOpen = false;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}