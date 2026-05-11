using TMPro;
using UnityEngine;

// This script is responsible for displaying the high scores in the game.

public class HighScoresUI : MonoBehaviour
{
    public TextMeshProUGUI normal1Text;
    public TextMeshProUGUI normal2Text;
    public TextMeshProUGUI normal3Text;

    public TextMeshProUGUI hard1Text;
    public TextMeshProUGUI hard2Text;
    public TextMeshProUGUI hard3Text;
    // This function is called when the object is enabled.
    void OnEnable()
    {
        // Update the high scores when the object is enabled.
        normal1Text.text = "1. Wave " + PlayerPrefs.GetInt("HighWave_Normal_1", 0);
        normal2Text.text = "2. Wave " + PlayerPrefs.GetInt("HighWave_Normal_2", 0);
        normal3Text.text = "3. Wave " + PlayerPrefs.GetInt("HighWave_Normal_3", 0);
        // Update the hard difficulty high scores.
        hard1Text.text = "1. Wave " + PlayerPrefs.GetInt("HighWave_Hard_1", 0);
        hard2Text.text = "2. Wave " + PlayerPrefs.GetInt("HighWave_Hard_2", 0);
        hard3Text.text = "3. Wave " + PlayerPrefs.GetInt("HighWave_Hard_3", 0);
    }
}