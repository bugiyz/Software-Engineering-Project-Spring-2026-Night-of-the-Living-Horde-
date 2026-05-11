using UnityEngine;
using UnityEngine.SceneManagement;

// This script is responsible for managing the game over state and providing options to retry or return to the main menu.

public class GameOverManager : MonoBehaviour
{
    // Reset the game stats and load the main level.
    public void TryAgain()
    {
        // Reset the game stats.
        if (GameStats.Instance != null)
            GameStats.Instance.ResetStats();
        // Load the main level.
        SceneManager.LoadScene("MainLevel");
    }

    public void LoadMainMenu()
    {
        // Reset the game stats.
        if (GameStats.Instance != null)
            GameStats.Instance.ResetStats();
        // Load the main menu.
        SceneManager.LoadScene("MainMenu");
    }
}