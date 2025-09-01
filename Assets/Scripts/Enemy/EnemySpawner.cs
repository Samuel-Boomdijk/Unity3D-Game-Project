using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    //References for the player
    private Transform player;            // Reference to the player
    public TMP_Text waveText;              // The player UI Text to display the wave number
    public TMP_Text enemiesLeftText;       // The player UI Text to display remaining enemies

    // Enemy Prefabs
    public GameObject zombiePrefab;
    public GameObject zombieGirlPrefab;
    public GameObject zombieCaptainPrefab;

    //Enemy Spawn points and interval
    public Transform[] spawnPoints;    // Array of spawn points (empty GameObjects in the scene)
    private float spawnInterval = 2f;    // Time between enemy spawns
    private float cooldownDuration = 0.5f;    //Duration of spawn cooldown
    private float cooldownTimer = 0f;   // Timer to track cooldown

    //Wave logic
    private int waveNumber = 1;               // Current wave number
    private int maxEnemiesForWave = 50;       // Max enemies for the current wave (50 is the default)
    private int overallEnemySpawnedInWaveCount = 0;      // Total enemies spawned in the wave
    private int remainingEnemies;

    //Everything for the Zombie
    private int currentZombieCount = 0;
    private float zombieSpawnRadius = 20f;     // Maximum spawn distance from the player
    private float zombieMinSpawnRadius = 5f;   // Minimum spawn distance from the player
    private int maxZombies = 100;         // Maximum number of zombies allowed in the scene


    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;

        remainingEnemies = maxEnemiesForWave;

        UpdatePlayerUI();

        // Start spawning zombies repeatedly
        InvokeRepeating(nameof(SpawnZombie), 2f, spawnInterval);
    }

    private void Update()
    {
        // Reduce the cooldown timer over time
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // Check if the wave is complete
        if (remainingEnemies == 0 && currentZombieCount == 0)
        {
            StartNextWave();
        }
    }

    private void SpawnZombie()
    {
        // Only spawn if there are remaining enemies in the wave
        if ((currentZombieCount >= maxZombies) || (currentZombieCount >= maxEnemiesForWave) || (overallEnemySpawnedInWaveCount >= maxEnemiesForWave))
        {
            return;
        }
        // Check if the cooldown timer is still active
        else if (cooldownTimer > 0f)
        {
            return;
        }

        // Randomly select a spawn point from the array
        Transform selectedSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 zombieSpawnPosition = selectedSpawnPoint.position;

        //Check that the position is within the NavMesh
        if (UnityEngine.AI.NavMesh.SamplePosition(zombieSpawnPosition, out UnityEngine.AI.NavMeshHit hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            // Determine whether to spawn a regular zombie, zombie girl, or zombie captain
            GameObject zombieToSpawn = Random.value < 0.1f ? zombieCaptainPrefab : (Random.value < 0.3f ? zombieGirlPrefab : zombiePrefab); // 10% chance for ZombieCaptain, 20% for ZombieGirl

            // Instantiate the zombie at the spawn position
            GameObject zombie = Instantiate(zombieToSpawn, zombieSpawnPosition, Quaternion.identity);
            
            // Increase the zombie count
            currentZombieCount++;
            overallEnemySpawnedInWaveCount++;

            // Attach an event to reduce the count when the zombie dies
            ZombieHealth zombieHealth = zombie.GetComponent<ZombieHealth>();
            if (zombieHealth != null)
            {
                zombieHealth.OnDeath += onZombieKilled;
            }
        }
        else
        {
            Debug.LogWarning("Position is not on NavMesh. Skipping spawn.");
        }
    }

    //Method to spawn enemy at random location
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomDirection;

        do
        {
            // Generate a random direction within the spawn radius
            randomDirection = Random.insideUnitSphere * zombieSpawnRadius;
            randomDirection.y = 0f; // Keep spawn positions on the ground
        }
        while (randomDirection.magnitude < zombieMinSpawnRadius); // Ensure it's outside the min spawn radius

        // Offset by the player's position to spawn near them
        return player.position + randomDirection;
    }

    void onZombieKilled()
    {
        currentZombieCount--;
        remainingEnemies --;
        UpdatePlayerUI();

        // Start the cooldown timer after a zombie is killed
        cooldownTimer = cooldownDuration;
    }

    //MEthod to start a new wave
    private void StartNextWave()
    {
        waveNumber++;
        overallEnemySpawnedInWaveCount = 0;
        maxEnemiesForWave += 50; // Increase max enemies for the new wave
        remainingEnemies = maxEnemiesForWave;

        // Reduce the spawn interval to spawn enemies faster
        spawnInterval = Mathf.Max(0.5f, spawnInterval - 0.2f); // Min spawn interval of 0.5 seconds

        // Restart zombie spawning with the new interval
        CancelInvoke(nameof(SpawnZombie));
        InvokeRepeating(nameof(SpawnZombie), 2f, spawnInterval);

        UpdatePlayerUI();
    }

    //Update the player UI with relevant info
    private void UpdatePlayerUI()
    {
        waveText.text = "Current Wave: " + waveNumber;
        enemiesLeftText.text = "Enemies left to kill: " + remainingEnemies;
    }
}
