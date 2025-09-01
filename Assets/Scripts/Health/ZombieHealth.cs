using System.Collections;
using UnityEngine;

public class ZombieHealth : EnemyHealth
{
    public delegate void ZombieDeathHandler();
    public event ZombieDeathHandler OnDeath;

    public EnemyAI.ZombieType zombieType;   //Type of zombie assigned in inspector for ease of use

    public GameObject dropCubePrefab; // Reference to the red cube prefab
    private float dropChance = 0.15f;  // 15% chance to drop the cube

    protected override void Start()
    {
        base.maxHealth = 100f;   //Set the maxHealth of the Zombie
        base.Start();

        // Set the points based on zombie type
        switch (zombieType)
        {
            case EnemyAI.ZombieType.Armored:
                scoreValue = 50; // Medium points for Armored Zombies
                break;
            case EnemyAI.ZombieType.Captain:
                scoreValue = 20; // More points for Zombie Captain
                break;
            case EnemyAI.ZombieType.Girl:
                scoreValue = 10; // Medium points for Armored Zombies
                break;
            case EnemyAI.ZombieType.Regular:
            default:
                scoreValue = 5; // Default points for regular zombies
                break;
        }
    }

    protected override void Die()
    {
        base.Die();

        // Trigger the OnDeath event
        if (OnDeath != null)
        {
            OnDeath.Invoke();
        }

        // Check if the cube should drop
        if (Random.value < dropChance)
        {
            // Spawn the red cube at the zombie's position
            Vector3 spawnPosition = transform.position;
            spawnPosition.y = transform.position.y + 0.5f;
            Quaternion spawnRotation = Quaternion.identity;
            Instantiate(dropCubePrefab, spawnPosition, spawnRotation);
        }
    }
}
