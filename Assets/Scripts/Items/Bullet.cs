using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    private float lifetime = 3f; // How long the bullet lasts before disappearing
    private float damage;     // Damage dealt by the bullet - based one the weapon

    private float rayDuration = 0.1f; // How long the ray is visible
    private LineRenderer lineRenderer; // Reference to the Line Renderer

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);  // Destroy the bullet after it exists for 'lifetime' seconds
    }

    private void update()
    {
        // Freeze Axis based on player direction
        Vector3 currentPosition = transform.position;
        transform.position = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z);
    }

    //The ray to see bullet trajectory
    public void FireRay(Vector3 firePoint, Vector3 hitPoint)
    {
        StartCoroutine(ShootRay(firePoint, hitPoint));
    }

    private IEnumerator ShootRay(Vector3 firePoint, Vector3 hitPoint)
    {
        // Set the positions of the Line Renderer
        lineRenderer.SetPosition(0, firePoint); // Start of the ray (firePoint)
        lineRenderer.SetPosition(1, hitPoint); // End of the ray (hitPoint)

        // Enable the Line Renderer to make it visible
        lineRenderer.enabled = true;

        // Wait for the duration of the ray effect
        yield return new WaitForSeconds(rayDuration);

        // Disable the Line Renderer after the duration
        lineRenderer.enabled = false;
    }

    //Setter that weapon can call to determine bullet's damage
    public void setDamage(float damage)
    {
        this.damage = damage;
    }
}
