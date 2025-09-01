using NUnit.Framework;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    //Types of Zombie
    public enum ZombieType
    {
        Regular,
        Girl,
        Captain,
        Armored
    }

    [Header("Enemy Multipliers")]
    public float damageDealtMultiplier = 1f; // Multiplier to modify damage dealt

    public GameObject armoredZombiePrefab;  // Reference to the Armored Zombie prefab
    private Vector3 leftOffset, rightOffset;

    private GameObject player;              //reference to the player
    private NavMeshAgent nav;               // Reference to the nav mesh agent.
    private Animator anim;                      // Reference to the animator component.

    private float attackRange = 3f;  // Distance at which the zombie can attack
    private float timeBetweenAttacks = 1.5f; // Delay between attacks
    private float attackDamage = 5f; // Damage dealt to the player per attack

    private bool isAttacking = false; // To control attack cooldown
    private bool hasScreamed = false;  // Used to ensure the Captain only screams once
    private float screamCooldown = 10f;  // Cooldown between screams for the Captain

    private float damageDelay = 0.85f;  // Wait time before applying knockback

    // Static variable to track the number of active armored zombies
    public static int activeArmoredZombies = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        anim = GetComponent<Animator>();
        // Example of dynamic offsets based on the Captain's position
        leftOffset = new Vector3(-2f, 0f, 0f);  // Left side of Captain
        rightOffset = new Vector3(2f, 0f, 0f);  // Right side of Captain
    }
    private void Awake()
    {
        // Set up the references.
        player = GameObject.FindGameObjectWithTag("Player");
        nav = GetComponent<NavMeshAgent>();
    }

    private enum States
    {
        Chase,   // The zombie runs after player
        Attack,  // The zombie attacks player
        Scream,  // Zombie screams to spawn zombies (Only Captain type)
    }

    private States _state = States.Chase; // Default state

    private void Update()
    {
        if (!nav.enabled) {return;}

        //Calculate distance to the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        //FSM
        switch (_state)
        {
            case States.Chase:
                ChaseBehavior(distanceToPlayer);
                break;
            case States.Attack:
                AttackBehavior(distanceToPlayer);
                break;
            case States.Scream:
                ScreamBehaviour();
                break;
        }
    }
    
    //Zombie chasing
    private void ChaseBehavior(float distanceToPlayer)
    {
        nav.isStopped = false;
        nav.SetDestination(player.transform.position); // Chase the player

        if (distanceToPlayer <= attackRange)
        {
            //If the Zombie is a captain, scream instead
            if(GetComponent<ZombieHealth>().zombieType == ZombieType.Captain)
            {
                _state = States.Scream; // Switch to attack state if zombie is in range
            }
            else
            {
                _state = States.Attack; // Switch to attack state if zombie is in range
            }
        }
    }

    //Zombie attacking
    private void AttackBehavior(float distanceToPlayer)
    {
        nav.isStopped = true; // Stop moving to attack

        if (!isAttacking)
        {
            StartCoroutine(AttackPlayer()); // Perform the attack
        }

        // Transition back to chase if the player moves out of range
        if (distanceToPlayer > attackRange)
        {
            _state = States.Chase;
        }
    }

    //Behaviour for when the Captain screams
    private void ScreamBehaviour()
    {
        if(!hasScreamed)
        {
            hasScreamed = true;
            nav.isStopped = true;
            anim.SetTrigger("Scream");
            // Manually define the spawn positions (relative to the Captain)
            Vector3 leftSpawnPosition = transform.position + leftOffset;
            Vector3 rightSpawnPosition = transform.position + rightOffset;

            // Check and spawn Armored Zombies at both positions
            StartCoroutine(spawnArmoredZombie(leftSpawnPosition));
            StartCoroutine(spawnArmoredZombie(rightSpawnPosition));

            StartCoroutine(ScreamCooldown());
            _state = States.Chase;
        }
    }

    //Helper method to spawn zombies
    private IEnumerator spawnArmoredZombie(Vector3 spawnPosition)
    {
        // Check if there are already 2 armored zombies alive
        if (activeArmoredZombies >= 2)
        {
            yield break;  // Exit the function if the limit is reached
        }

        yield return new WaitForSeconds(2f);    //Wait for animation to play and a slight delay for effect
        if (UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out UnityEngine.AI.NavMeshHit hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            GameObject armoredZombie = Instantiate(armoredZombiePrefab, hit.position, Quaternion.identity);
            // Increase the count of active armored zombies
            activeArmoredZombies++;

            // Listener for the armored zombie's death event to decrease the count when it dies
            ZombieHealth zombieHealth = armoredZombie.GetComponent<ZombieHealth>();
            if (zombieHealth != null)
            {
                zombieHealth.OnDeath += () => activeArmoredZombies--;  // Decrease the count when the zombie dies
            }
        }
    }

    //Helper method to reset scream
    private IEnumerator ScreamCooldown()
    {
        yield return new WaitForSeconds(screamCooldown);
        hasScreamed = false;
    }

    // Helper method to help Zombie attack
    private IEnumerator AttackPlayer()
    {
        isAttacking = true;

        //Stop walking animation
        anim.SetBool("isAttacking", isAttacking);   //Start attacking animation

        // Apply knockback and disable movement during animation
        StartCoroutine(damagePlayer());

        // Wait for the cooldown before the next attack
        yield return new WaitForSeconds(timeBetweenAttacks);
        isAttacking = false;
        anim.SetBool("isAttacking", isAttacking);   //Stop attacking animation

    }

    // Coroutine to apply knockback after the animation starts
    private IEnumerator damagePlayer()
    {
        // Wait for the specified delay before applying knockback
        yield return new WaitForSeconds(damageDelay);  // Wait for knockbackDelay seconds

        //Calculate distance to the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if(distanceToPlayer <= attackRange) 
        {
            // Assume the player has a PlayerHealth script
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage * damageDealtMultiplier); // Inflict damage on the player
            }
        }
    }

    // Method to make enemies run from smoke
    public void AvoidSmoke(Vector3 smokeCenter, float smokeRadius)
    {
        if (Vector3.Distance(transform.position, smokeCenter) < smokeRadius)
        {
            Vector3 runDirection = (transform.position - smokeCenter).normalized;
            Vector3 safePoint = transform.position + runDirection * smokeRadius;

            nav.SetDestination(safePoint); // Run away using NavMeshAgent
        }
    }
}
