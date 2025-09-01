using UnityEngine;
using UnityEngine.UI;

public class BaseHealth : MonoBehaviour
{
    protected float maxHealth;
    protected float currentHealth;
    protected bool isDead = false;  // To prevent the death process from repeating

    // Initialize health
    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    // Method to take damage
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return; // Prevent further damage if the player is already dead

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);  // Ensure health doesn't go below 0

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    // Method to handle death (Will be overridden by child classes)
    protected virtual void Die()
    {
        isDead = true;
    }
}
