using TMPro;
using UnityEngine;

// This script is responsible for managing the game over UI and displaying the final score.
public class GameOverUI : MonoBehaviour
{
    public TextMeshProUGUI zombiesKilledText;
    public TextMeshProUGUI wavesSurvivedText;

    public TextMeshProUGUI high1Text;
    public TextMeshProUGUI high2Text;
    public TextMeshProUGUI high3Text;

    void Start()
    {
        // Initialize the game over UI with the final score.
        if (GameStats.Instance != null)
        {
            // Display the number of zombies killed.
            zombiesKilledText.text =
                "Zombies Killed: " + GameStats.Instance.zombiesKilled;

            // Display the number of waves survived.
            wavesSurvivedText.text =
                "Waves Survived: " + GameStats.Instance.wavesSurvived;
        }
        // Display the high scores.
        var difficulty = SettingsManager.Instance != null
            ? SettingsManager.Instance.difficulty
            : SettingsManager.Difficulty.Easy;
        // If the difficulty is easy, display a message indicating that no high scores are available.
        if (difficulty == SettingsManager.Difficulty.Easy)
        {
            high1Text.text = "Not wasting memory, it's on Easy";
            high2Text.text = "";
            high3Text.text = "";
            return;
        }
        // Otherwise, display the high scores for the current difficulty.
        string keyPrefix = "HighWave_" + difficulty + "_";

        high1Text.text = "1. Wave " + PlayerPrefs.GetInt(keyPrefix + "1", 0);
        high2Text.text = "2. Wave " + PlayerPrefs.GetInt(keyPrefix + "2", 0);
        high3Text.text = "3. Wave " + PlayerPrefs.GetInt(keyPrefix + "3", 0);
    }
}