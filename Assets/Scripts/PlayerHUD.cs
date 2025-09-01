using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    public TMP_Text scoreText; // Assign the ScoreText UI element in the inspector

    private void Update()
    {
        if (ScoreManager.instance != null)
        {
            // Update the score text
            scoreText.text = $"Player Score: {ScoreManager.instance.GetScore()}";
        }
    }
}
