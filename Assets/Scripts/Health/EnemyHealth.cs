using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.AI;

public class EnemyHealth : BaseHealth
{
    [Header("Enemy Multipliers")]
    public float healthMultiplier = 1f;  // Multiplier to increase health for specific enemy types
    public float damageMultiplier = 1f; // Multiplier to modify damage taken

    private Slider healthBarSlider;  // Reference to the health bar slider
    private GameObject healthBarObject;  // Parent object of the health bar slider
    private Animator anim;
    private NavMeshAgent navMeshAgent; // Reference to the NavMeshAgent

    private float fadeDuration = 2f; // Duration of the fade-out
    private Renderer[] renderers; // Array to store all renderers on the enemy

    protected int scoreValue;

    protected override void Start()
    {
        maxHealth *= healthMultiplier;  // Apply the health multiplier
        base.Start();   // Call the base class' Start method

        // Find the health bar component in the enemy's child objects
        healthBarObject = transform.Find("EnemyHealthHUD").gameObject;
        healthBarSlider = healthBarObject.transform.Find("HealthBar").GetComponent<Slider>();

        // Initialize the health bar
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth / 1.0f;
            healthBarObject.SetActive(false); // Hide the health bar initially
        }

        anim = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        // Find all renderers on the enemy
        renderers = GetComponentsInChildren<Renderer>();
    }

    // This method updates the health bar's position above the enemy's head
    protected virtual void Update()
    {
        if (healthBarObject != null)
        {
            Vector3 worldPos = Camera.main.WorldToScreenPoint(transform.position);  //Keep the healthbar above the zombie's head

            // Make the health bar always face the camera
            if(Camera.main != null)
            {
                // Stop the health bar from rotating:
                healthBarObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }

        }
    }

    public override void TakeDamage(float damage)
    {
        if (isDead) return; // Prevent further damage if already dead

        // Apply damage multiplier and reduce health
        float adjustedDamage = damage / damageMultiplier;
        base.TakeDamage(adjustedDamage); // Call the base TakeDamage method

        // Update the health bar slider value when damage is taken
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth /1.0f;
        }

        if ((currentHealth < maxHealth) && (currentHealth > 0))
        {
            healthBarObject.SetActive(true);  // Show the health bar if health is not full
        }
        else
        {
            healthBarObject.SetActive(false);
        }
    }

    protected override void Die()
    {
        // Call the base Die method
        base.Die();

        // Add points to the score
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddPoints(scoreValue); // Example: 10 points per zombie
        }

        // Disable the NavMeshAgent to stop movement
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;  // Stop the agent from moving
            navMeshAgent.enabled = false;  // Disable the NavMeshAgent
        }

        // Disable all colliders on the zombie
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        anim.SetTrigger("Die"); // Play death animation

        // Start fading out and destroy the zombie
        StartCoroutine(FadeOutAndDestroy());
    }

    // Coroutine to fade the zombie out and destroy it
    private IEnumerator FadeOutAndDestroy()
    {
        yield return new WaitForSeconds(2f); //Wait so that the death aniamtion can play

        // Get the materials from all renderers
        Material[] materials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            materials[i] = renderers[i].material;
        }

        // Gradually fade out over time
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            foreach (Material mat in materials)
            {
                if (mat.HasProperty("_BaseColor"))
                {
                    Color baseColor = mat.GetColor("_BaseColor");
                    baseColor.a = alpha; // Adjust the alpha value
                    mat.SetColor("_BaseColor", baseColor);
                }
            }
            yield return null;
        }

        // Destroy the zombie after fading out
        Destroy(gameObject);
    }
}
