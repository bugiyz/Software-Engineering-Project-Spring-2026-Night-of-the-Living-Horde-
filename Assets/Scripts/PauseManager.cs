using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject settingsPanel;

    private bool isPaused = false;
    private bool isSettingsOpen = false;

    void Start()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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
        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;
        isSettingsOpen = false;
    }

    public void ResumeGame()
    {
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