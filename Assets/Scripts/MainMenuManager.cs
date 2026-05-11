using UnityEngine;
using UnityEngine.SceneManagement;

// This script is responsible for managing the main menu of the game.

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject highScoresPanel;
    public GameObject tutorialPanel;
    public GameObject creditsPanel;
    public SettingsUI settingsUI;

    void Start()
    {
        // Initialize the panels.
        ReturnToMain();
    }
    // Function to open the settings panel.
    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        highScoresPanel.SetActive(false);
        tutorialPanel.SetActive(false);
        // Synchronize the settings UI.
        if (settingsUI != null)
        {
            settingsUI.SyncUI();
        }
    }
    // Function to open the high scores panel.
    public void OpenHighScores()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        highScoresPanel.SetActive(true);
        tutorialPanel.SetActive(false);
    }
    // Function to open the tutorial panel.
    public void OpenTutorial()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        highScoresPanel.SetActive(false);
        tutorialPanel.SetActive(true);
    }

    // Function to open the credits panel.
    public void OpenCredits()
{
    mainMenuPanel.SetActive(false);
    settingsPanel.SetActive(false);
    highScoresPanel.SetActive(false);
    tutorialPanel.SetActive(false);
    creditsPanel.SetActive(true);
}
    // Function to return to the main menu.
    public void ReturnToMain()
    {
        settingsPanel.SetActive(false);
        highScoresPanel.SetActive(false);
        tutorialPanel.SetActive(false);
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
    // Function to play the game.
    public void PlayGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("MainLevel");
    }
    // Function to quit the game.
    public void QuitGame()
    {
        Debug.Log("Quit button clicked");

        Time.timeScale = 1f;
        AudioListener.pause = false;
// Quit the game.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}