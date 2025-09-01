using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : BaseHealth
{
    private Animator anim;  // Reference to the animator component.
    private PlayerMovement playerMovement; // Reference to the movement script
    private GameObject gameOverUI;  // Reference to the Panel
    private GameObject playerUI;      //Reference to the Canvas for the gameover screem
    private GameObject gameOverPanel;      //Reference to the panel
    private Slider healthBarSlider;  // Reference to the health bar slider
    public TMP_Text gameOverMenuScoreText; // Assign the PauseMenuScoreText UI element in the inspector
    private float deathAnimationTime = 4f; // Duration of death animation


    protected override void Start()
    {
        base.maxHealth = 100f;    //Set the maxHealth of the Player

        base.Start(); // Call the base class' Start method

        // Get the Animator component on the player
        anim = GetComponent<Animator>();

        playerMovement = GetComponent<PlayerMovement>();

        gameOverUI = GameObject.FindWithTag("gameOverUI");
        playerUI = GameObject.FindWithTag("playerUI");

        healthBarSlider = playerUI.transform.Find("HealthBar").GetComponent<Slider>();
        gameOverPanel = gameOverUI.transform.Find("gameOverPanel").gameObject;
        gameOverPanel.SetActive(false);

        // Initialize the health bar
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth / 1.0f;
        }
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);    //Use parent method

        if(base.currentHealth > 0)
        {
            playerMovement.HitReact();  // Trigger hit reaction
        }

        // Update the health bar slider value when damage is taken
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth /1.0f;
        }
    }

    protected override void Die()
    {
        base.Die();

        // Trigger death animation
        anim.SetTrigger("die");

        // Disable player movement and actions
        GetComponent<PlayerController>().enabled = false;
        GetComponent<PlayerMovement>().enabled = false;

        // Trigger screen fade
        StartCoroutine(FadeToBlack());

        // Start game over sequence after death animation
        StartCoroutine(GameOver());
    }

    private IEnumerator FadeToBlack()
    {
        // Assuming you have a UI Image for the fade effect (black screen)
        Image fadeImage = GameObject.Find("fadeImage").GetComponent<Image>();
        float elapsedTime = 0f;

        // Set the target alpha
        float targetAlpha = 0.95f;

        // Fade to black (alpha = 1)
        while (elapsedTime < deathAnimationTime)
        {
            fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(0, targetAlpha, elapsedTime / deathAnimationTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, targetAlpha);
    }

    private IEnumerator GameOver()
    {
        // Wait for the death animation to finish
        yield return new WaitForSeconds(deathAnimationTime);

        // Show Game Over UI and pause the game
        gameOverPanel.SetActive(true);
        playerUI.SetActive(false);
        Time.timeScale = 0f;  // Pause the game

        if (ScoreManager.instance != null)
        {
            // Update the score text
            gameOverMenuScoreText.text = $"Your Final Score is: {ScoreManager.instance.GetScore()}";
        }
    }
}
