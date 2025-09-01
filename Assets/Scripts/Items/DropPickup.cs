using UnityEngine;

public class DropPickup : MonoBehaviour
{
    private float lifespan = 10f; // Time before the cube disappears
    public Weapon[] availableWeapons; // List of all available weapon scriptable objects
    private float[] dropRates = {0.25f, 0.22f, 0.2f, 0.15f, 0.12f, 0.05f, 0.01f}; // Percent chance for each weapon to drop

    private void Start()
    {
        // Destroy the cube after the lifespan expires
        Destroy(gameObject, lifespan);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory inventoryScript = other.GetComponent<PlayerInventory>();

            if (inventoryScript != null)
            {
                // Add a random weapon to the player's inventory
                Weapon weaponToAdd = GetRandomWeapon();
                if (weaponToAdd != null)
                {
                     // Force initialization for the new weapon if necessary
                    Weapon weaponScript = weaponToAdd.GetComponent<Weapon>();
                    if (weaponScript != null)
                    {
                        weaponScript.Awake();
                    }

                    inventoryScript.addItemToSlot(weaponToAdd, FindEmptySlot(inventoryScript));
                }
                else
                {
                    Debug.LogWarning("No weapon could be added.");
                }

                // Destroy the pickup after it has been collected
                Destroy(gameObject);
            }
        }
    }

    private Weapon GetRandomWeapon()
    {
        if (availableWeapons == null || availableWeapons.Length == 0 || dropRates == null || dropRates.Length != availableWeapons.Length)
        {
            Debug.LogWarning("Weapon list or drop rates not configured properly!");
            return null;
        }

        // Generate a random value between 0 and 1
        float randomValue = Random.value;

        // Determine which weapon to drop based on drop rates
        float cumulativeProbability = 0f;
        for (int i = 0; i < availableWeapons.Length; i++)
        {
            cumulativeProbability += dropRates[i];
            if (randomValue <= cumulativeProbability)
            {
                return availableWeapons[i];
            }
        }
        return null; // No weapon found
    }

    private int FindEmptySlot(PlayerInventory inventoryScript)
    {
        // Loop through all inventory slots and return the first empty one
        for (int i = 0; i < inventoryScript.getInventorySlots().Length; i++)
        {
            if (inventoryScript.getItemFromSlot(i) == null)
            {
                return i;
            }
        }
        return -1; // No empty slot found
    }
}
