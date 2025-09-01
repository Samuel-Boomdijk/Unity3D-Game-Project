using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance; // Singleton instance

    private int currentScore = 0; // Player's current score

    private void Awake()
    {
        // Ensure only one ScoreManager exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate ScoreManagers
        }
    }

    // Method to add points
    public void AddPoints(int points)
    {
        currentScore += points;
    }

    // Getter for the current score
    public int GetScore()
    {
        return currentScore;
    }

    // Resets the score (e.g., for a new game)
    public void ResetScore()
    {
        currentScore = 0;
    }
}
