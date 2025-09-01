using UnityEngine;
using System.Collections;
using System.Linq;

public class PlayerMovement : MonoBehaviour
{
    private float defaultSpeed = 15f;    //The default speed that the player will move at.
    private float currentSpeed;          // The current speed of the player
    private Vector3 movement;      //The vector to store the direction of the player's movement
    private Rigidbody playerRigidbody;      // Reference to the player's rigidbody.
    private Animator anim;                      // Reference to the animator component.
    private float knockbackForce = 15f;      // Force applied during knockback
    private float knockbackDuration = 2f;   // Duration of knockback
    private bool isKnockedBack = false;    // Whether the player is knocked back
    private float knockbackDelay = 0.15f;  // Wait time before applying knockback (in seconds)
    private PlayerController controllerScript; // Reference to the controller script
    private Transform cameraTransform; // Reference to the camera

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Set up references.
        playerRigidbody = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        controllerScript = GetComponent<PlayerController>();
        currentSpeed = defaultSpeed;
        cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    private void Update()
    {
        if (isKnockedBack) {
            return;  // Don't process movement if player is knocked back
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Turning(h, v);
        Animating(h, v);
    }

    private void FixedUpdate()
    {
        if (isKnockedBack) {
            return;  // Don't process movement if player is knocked back
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Move the player around the scene.
        Move(h, v);
    }

    private void Move(float h, float v)
    {
        // Get the camera's forward and right directions
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Ignore the vertical axis of the camera
        forward.y = 0f;
        right.y = 0f;

        // Normalize the vectors to ensure consistent movement speed
        forward.Normalize();
        right.Normalize();

        // Calculate the movement direction based on camera orientation
        movement = forward * v + right * h;

        // Normalise the movement vector and make it proportional to the speed per second
        movement = movement.normalized * currentSpeed * Time.fixedDeltaTime;

        // Check for walls using a raycast
        if (IsNearWall())
        {
            // Reduce speed near walls
            movement *= 0.5f; // Scale down the movement speed
        }

        // Move the player to it's current position plus the movement.
        playerRigidbody.MovePosition(transform.position + movement);
    }

    private void Turning(float h, float v)
    {
        // Check if there's any movement input
        if (h != 0f || v != 0f)
        {
            // Get the camera's forward and right directions
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            // Ignore the vertical axis of the camera
            forward.y = 0f;
            right.y = 0f;

            // Normalize the vectors
            forward.Normalize();
            right.Normalize();

            // Calculate the movement direction based on input and camera orientation
            Vector3 moveDirection = forward * v + right * h;

            // Rotate the player to face the movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
        else
        {
            // Snap to the nearest cardinal direction when there's no input
            Vector3 currentForward = transform.forward;
            currentForward.y = 0f; // Ignore vertical component
            currentForward.Normalize();

            // Calculate the angle to the nearest cardinal direction (0, 90, 180, 270)
            float angle = Mathf.Atan2(currentForward.x, currentForward.z) * Mathf.Rad2Deg;
            float snappedAngle = Mathf.Round(angle / 90f) * 90f; // Snap to nearest 90 degrees

            // Apply the snapped rotation
            Quaternion snappedRotation = Quaternion.Euler(0f, snappedAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, snappedRotation, Time.deltaTime * 10f);
        }
    } 
   

    private void Animating(float h, float v)
    {
        // Create a boolean that is true if either of the input axes is non-zero.
        bool running = h != 0f || v != 0f;

        // Tell the animator whether or not the player is walking.
        anim.SetBool("isRunning", running);
    }

    // Helper method to check for walls
    private bool IsNearWall()
    {
        float checkDistance = 0.5f; // Adjust distance based on player's size
        RaycastHit hit;

        // Check in the direction the player is moving
        if (Physics.Raycast(transform.position, movement.normalized, out hit, checkDistance))
        {
            // If the ray hits a wall, return true
            if (hit.collider.CompareTag("Wall"))
            {
                return true;
            }
        }

        return false;
    }

    // Call this method when the player is hit
    public void HitReact()
    {
        if (isKnockedBack) return;  // Prevent multiple hits while already in knockback

        // Disable player movement during knockback
        isKnockedBack = true;
        controllerScript.enabled = false;

        // Trigger the hit react animation
        anim.SetTrigger("hitReact");

        // Apply knockback and disable movement during animation
        StartCoroutine(ApplyKnockbackWithDelay());


        // Call a coroutine to reset movement after knockback is finished
        StartCoroutine(ResetMovementAfterKnockback());
    }

    // Coroutine to apply knockback after the animation starts
    private IEnumerator ApplyKnockbackWithDelay()
    {
        // Wait for the specified delay before applying knockback
        yield return new WaitForSeconds(knockbackDelay);  // Wait for knockbackDelay seconds

        // Apply knockback force in the opposite direction the player is facing
        if (Camera.main  != null)
        {
            // Get the direction the player is facing (opposite direction)
            Vector3 knockbackDirection = -transform.forward; // Opposite of the player's forward direction
            knockbackDirection.y = 0f;  // Keep it horizontal (no vertical knockback)

            // Normalize the direction to ensure consistent force
            knockbackDirection.Normalize();

            // Apply the knockback force in the correct direction
            playerRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogError("No camera found! Please assign the main camera.");
        }
    }

    // Reset movement and animation after the knockback animation finishes
    private IEnumerator ResetMovementAfterKnockback()
    {
        // Wait for the hit reaction animation to finish
        yield return new WaitForSeconds(knockbackDuration);

        // Set the player to idle state
        anim.SetBool("isRunning", false);  // reset to Idle

        // Wait for a few seconds
        //yield return new WaitForSeconds(1);

        // Allow movement again
        isKnockedBack = false;
        controllerScript.enabled = true;
    }

    //Setter method to change the player's speed based on what the player is holding
    public void setPlayerSpeedByItem(WeaponStats.WeaponType? typeOfItemBeingHeld)
    {
        WeaponStats.WeaponType[] listOfDefaultSpeedTypes = 
        {WeaponStats.WeaponType.Pistol, WeaponStats.WeaponType.SMG};

        if(typeOfItemBeingHeld == null || listOfDefaultSpeedTypes.Contains(typeOfItemBeingHeld.Value))
        {
            currentSpeed = defaultSpeed;
        }
        else if(typeOfItemBeingHeld == WeaponStats.WeaponType.Rifle)
        {
            currentSpeed = defaultSpeed - 5f;
        }
        else if(typeOfItemBeingHeld == WeaponStats.WeaponType.Shotgun)
        {
            currentSpeed = defaultSpeed - 8f;
        }
        else if(typeOfItemBeingHeld == WeaponStats.WeaponType.MachineGun)
        {
            currentSpeed = defaultSpeed - 10f;
        }
    }
}
