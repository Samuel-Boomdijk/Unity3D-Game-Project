using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    private Animator anim;                      // Reference to the animator component.
    private Rigidbody playerRigidbody;      // Reference to the player's rigidbody.
    private PlayerMovement movementScript;  // Reference to the movement script
    private PlayerInventory inventoryScript;  // Reference to the inventory script
    private List<GameObject> itemList = new List<GameObject>(); // List to hold all items being held by the player
    private GameObject playerSpawnPoint;    //Empty object taht references the location of the player spawnpoint

    private Item currentItem; // Currently equipped item

    // Event to notify changes in currentItem
    public delegate void CurrentItemChangeHandler();
    public event CurrentItemChangeHandler onCurrentItemChange;

    //Set the player's position to the starting position
    private void Awake()
    {
        playerSpawnPoint = GameObject.FindWithTag("Respawn");
        if(playerSpawnPoint != null)
        {
            transform.position = playerSpawnPoint.transform.position;
        }
        else
        {
            Debug.LogWarning("Could not locate spawnPoint, Player will spawn anywhere");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Set up references.
        playerRigidbody = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        movementScript = GetComponent<PlayerMovement>();
        inventoryScript = GetComponent<PlayerInventory>();

        // Subscribe to item changes
        onCurrentItemChange += useCurrentItem;

        //Set all items that the player is holding inactive
        setItemVisibility(null);
    }

    private void Update()
    {
        useCurrentItem();
    }

    private void useCurrentItem()
    {
        //If there is no currentItem
        if(currentItem != null)
        {
            //Check if the current item is a gun
            if(currentItem.CompareTag("Gun"))
            {
                //Get the weapon stats ready
                Weapon weaponScript = currentItem.GetComponent<Weapon>();
                
                //If the weapon is not a grenade
                if(weaponScript != null)
                {
                    //Set speed based on the weapon being held
                    movementScript.setPlayerSpeedByItem(weaponScript.weaponStats.weaponType);

                    // Shoot gun with Z
                    if (Input.GetKeyDown(KeyCode.Z))
                    {
                        if (weaponScript.weaponStats.isAutomatic)
                        {
                            currentItem.Use("Shoot automatic gun");
                        }
                        else
                        {
                            currentItem.Use("Shoot non-automatic gun");
                        }
                    }

                    //If Z is lifted
                    if (Input.GetKeyUp(KeyCode.Z) && weaponScript.weaponStats.isAutomatic)
                    {
                        currentItem.Use("Stop shooting automatic gun");
                    }

                    // Reload gun with X
                    if (Input.GetKeyDown(KeyCode.X))
                    {
                        currentItem.Use("Reload gun");  // Call the Use method of the current weapon and tell it you want to reload 
                    }
                }
            }
            else
            {
                //Return player speed to default
                movementScript.setPlayerSpeedByItem(null);
            }
        }
        else
        {
            //Return player speed to default
            movementScript.setPlayerSpeedByItem(null);
        }
    }

    //Setter method for currentItem field
    public void setCurrentItem(Item item)
    {
        // Ensure all items are invisible before switching to a new item
        setItemVisibility(null);

        //Check if there is an item being passed through
        if(item != null)
        {
            currentItem = item;
            setItemVisibility(currentItem);   //Set current item visible

            //Check what item it is
            if(item.CompareTag("Gun"))
            {
                //Get the weapon stats ready
                Weapon weaponScript = currentItem.GetComponent<Weapon>();

                 // **Ensure ammo is initialized**
                if (weaponScript.getCurrentAmmo() == 0)
                {
                    currentItem.Use("Reload gun");
                }
                
                //Update animation
                StartCoroutine(updateAnimationLayer(weaponScript.weaponStats.weaponType));
            }
        }
        else
        {
            currentItem = null;
            StartCoroutine(updateAnimationLayer(null));
            setItemVisibility(null);
        }
        // Notify listeners that the currentItem has changed
        onCurrentItemChange?.Invoke();
    }

    // Coroutine to update animation layer
    private IEnumerator updateAnimationLayer(WeaponStats.WeaponType? typeofWeapon)
    {
        // Coroutine must have a return
        yield return new WaitForSeconds(0f);

        if(typeofWeapon == WeaponStats.WeaponType.Pistol || typeofWeapon == WeaponStats.WeaponType.SMG)
        {
            // Player now holding pistol or SMG
            setAnimationBools("isHoldingPistol");
        }
        else if(typeofWeapon == WeaponStats.WeaponType.Rifle)
        {
            // Player now holding Rifle
            setAnimationBools("isHoldingRifle");
        }
        else if(typeofWeapon == WeaponStats.WeaponType.Shotgun)
        {
            // Player now holding Shotgun
            setAnimationBools("isHoldingShotgun"); 
        }
        else if(typeofWeapon == WeaponStats.WeaponType.MachineGun)
        {
            // Player now holding machinegun
            setAnimationBools("isHoldingMachinegun");
        }
        else
        {
            //Set ALL to false
            setAnimationBools("");
        }
    }

    // Helper method to reset the other bools
    private void setAnimationBools(string boolToSet)
    {
        //List of all the weapon bools
        string[] weaponBools = 
        {
            "isHoldingPistol", "isHoldingSMG", "isHoldingRifle", 
            "isHoldingShotgun", "isHoldingMachinegun"
        };

        //Loop through all the bools
        foreach (string weaponBool in weaponBools)
        {
            //If it is the bool we want to set, make it active
            if (weaponBool == boolToSet)
            {
                anim.SetBool(weaponBool, true);
            }
            else
            //The rest will be set to false
            {
                anim.SetBool(weaponBool, false);
            }
        }
    }

    // Method to set item's visible when use and invisible when not in use
    private void setItemVisibility(Item itemToSetActive)
    {
        itemList.Clear(); // Clear the list to avoid duplicates

        // Perform a recursive search for the Weapons container
        Transform weaponsContainer = FindChildWithName(transform, "Weapons");
        if (weaponsContainer == null)
        {
            Debug.LogError("Weapons container not found in the player hierarchy!");
            return;
        }

        // Get all items in the Weapons container
        foreach (Transform child in weaponsContainer)
        {
            itemList.Add(child.gameObject);
        }

        // Check if itemToSetActive is null
        if (itemToSetActive == null)
        {
            foreach (GameObject itemObject in itemList)
            {
                itemObject.SetActive(false); // Deactivate all items
            }
            return;
        }

        // Set the desired item active and the rest inactive
        foreach (GameObject itemObject in itemList)
        {
            if (itemObject.CompareTag("Gun"))
            {
                Weapon weaponScript = itemObject.GetComponent<Weapon>();
                if (weaponScript != null && weaponScript == itemToSetActive)
                {
                    itemObject.SetActive(true); // Activate the desired weapon
                }
                else
                {
                    itemObject.SetActive(false); // Deactivate all other weapons
                }
            }
        }
    }

    // Recursively search for a child with a specific name
    private Transform FindChildWithName(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child; // Found the child
            }

            // Recursive call to check the child's children
            Transform result = FindChildWithName(child, childName);
            if (result != null)
            {
                return result; // Found in a deeper level
            }
        }
        return null; // Not found
    }

    // Recursive helper method to find all children with a tag resembling an item
    private void getAllItems(Transform parent, List<GameObject> items)
    {
        foreach (Transform child in parent)
        {
            // Check if the child has a tag that is associated with an item
            if (child.CompareTag("Gun"))
            {
                items.Add(child.gameObject);
            }

            // Recursively search the child's children
            if (child.childCount > 0)
            {
                getAllItems(child, items);
            }
        }
    }

    //Getter method to return the current Item
    public Item getCurrentItem()
    {
        return currentItem;
    }
}
