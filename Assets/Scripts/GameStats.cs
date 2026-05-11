using UnityEngine;

// This script is responsible for tracking and managing the game statistics.

public class GameStats : MonoBehaviour
{
    // Create a single instance of the GameStats class.
    public static GameStats Instance;

    public int zombiesKilled;
    public int wavesSurvived;

    void Awake()
    {
        // Ensure that there is only one instance of the GameStats class.
        if (Instance == null)
        {
            // Set the instance to this object.
            Instance = this;
            // Make the instance persistent across scene changes.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, destroy this one.
            Destroy(gameObject);
        }
    }
    // function to add a zombie kill
    public void AddZombieKill()
    {
        // Increment the number of zombies killed.
        zombiesKilled++;
    }
    // function to set the current wave
    public void SetWave(int wave)
    {
        // Set the number of waves survived.
        wavesSurvived = wave;
    }

    // function to save the high scores
    public void SaveHighScores()
    {
        // Save the high scores to PlayerPrefs.
        var difficulty = SettingsManager.Instance != null
            ? SettingsManager.Instance.difficulty
            : SettingsManager.Difficulty.Easy;
        // If the difficulty is easy, no high scores are saved.
        if (difficulty == SettingsManager.Difficulty.Easy)
            return;
        // Otherwise, save the high scores for the current difficulty.
        int[] highs = new int[3];
        string keyPrefix = GetDifficultyKeyPrefix();
        // Get the current high scores.
        highs[0] = PlayerPrefs.GetInt(keyPrefix + "1", 0);
        highs[1] = PlayerPrefs.GetInt(keyPrefix + "2", 0);
        highs[2] = PlayerPrefs.GetInt(keyPrefix + "3", 0);
        // Update the high scores if the current wave is higher.
        if (wavesSurvived > highs[0])
        {
            highs[2] = highs[1];
            highs[1] = highs[0];
            highs[0] = wavesSurvived;
        }
        // If the current wave is not higher than the first high score, check if it's higher than the second.
        else if (wavesSurvived > highs[1])
        {
            highs[2] = highs[1];
            highs[1] = wavesSurvived;
        }
        // If the current wave is not higher than the second high score, check if it's higher than the third.
        else if (wavesSurvived > highs[2])
        {
            highs[2] = wavesSurvived;
        }  
        // Save the updated high scores.
        PlayerPrefs.SetInt(keyPrefix + "1", highs[0]);
        PlayerPrefs.SetInt(keyPrefix + "2", highs[1]);
        PlayerPrefs.SetInt(keyPrefix + "3", highs[2]);
        // Save the changes to PlayerPrefs.
        PlayerPrefs.Save();
    }
    // function to get the difficulty key prefix
    string GetDifficultyKeyPrefix()
    {
        var difficulty = SettingsManager.Instance != null
            ? SettingsManager.Instance.difficulty
            : SettingsManager.Difficulty.Easy;

        return "HighWave_" + difficulty + "_";
    }
    // function to reset the game statistics
    public void ResetStats()
    {
        zombiesKilled = 0;
        wavesSurvived = 0;
    }
}