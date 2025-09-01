using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlot
    {
        private GameObject player;   // Reference to the Player
        private GameObject playerHUD;   // Reference to the PlayerHUD
        private Image slotBackground;   // Background image of the slot
        private Image slotIcon;         // Image that will show the icon
        private Image selectedFrame;    // Image of the frame that is used when the slot is selected
        private Item slotItem;      // Weapon in the slot (can be null)
        private TMP_Text ammoText;      // Ammo count (can be null)
        
        //Constructor
        public InventorySlot(int slotNumber)
        {
            // Locate the inventoryObject

            player = GameObject.FindWithTag("Player");
            playerHUD = GameObject.FindWithTag("playerUI");
            Transform inventoryObject = playerHUD.transform.Find("Inventory");

            // Get references to the background, icon, and ammo text
            string slotName = "InventorySlot" + slotNumber;
            slotBackground  = inventoryObject.Find(slotName).GetComponent<Image>();
            slotIcon = slotBackground.transform.Find("Icon").GetComponent<Image>();
            ammoText = slotBackground.transform.Find("ammoText").GetComponent<TMP_Text>();
            selectedFrame = slotBackground.transform.Find("selectedFrame").GetComponent<Image>();

            // Set initial states
            slotIcon.enabled = false; // Icon is hidden by default
            ammoText.text = "";       // No ammo text initially
            ammoText.enabled = false; // Hide ammo text
            selectedFrame.enabled = false;
        }

        // Method to set the item for this slot
        public void setItemInSlot(Item item)
        {
            slotItem = item;

            if (slotItem != null)
            {
                //If the item is a gun
                if(slotItem.CompareTag("Gun"))
                {
                    //Get the weapon stats ready
                    Weapon weaponScript = slotItem.GetComponent<Weapon>();

                    // **Initialize ammo if it hasn't been set**
                    if (weaponScript.getCurrentAmmo() == 0)
                    {
                        slotItem.Use("Reload gun"); // Fully reload the weapon
                    }

                    // Update the slot icon and ammo display
                    slotIcon.sprite = weaponScript.weaponStats.weaponIcon;
                    player.GetComponent<PlayerInventory>().setIconOrientation(weaponScript.weaponStats.weaponName, slotIcon);
                    slotIcon.enabled = true;

                    // Subscribe to ammo updates from the weapon
                    weaponScript.OnGunAmmoChanged += updateAmmoText;

                    if (ammoText != null)
                    {
                        //Check if the ammo is low
                        if(weaponScript.getCurrentAmmo() <= weaponScript.weaponStats.ammoThreshold)
                        {
                            ammoText.text = $"<color=#960000>{weaponScript.getCurrentAmmo()}</color> / {weaponScript.weaponStats.maxAmmo}";
                        }
                        else 
                        {
                            ammoText.text = $"<color=#FFFFFF>{weaponScript.getCurrentAmmo()}</color> / {weaponScript.weaponStats.maxAmmo}";
                        }
                        ammoText.enabled = true;
                    }
                    else
                    {
                        ammoText.text = "";
                        ammoText.enabled = false;
                    }
                }
            }
            else
            {
                removeItemInSlot();
            }
        }

        // Method to clear the slot (remove the weapon)
        public void removeItemInSlot()
        {
            slotItem = null;

            // Reset icon and ammo text
            slotIcon.sprite = null;
            slotIcon.enabled = false;
            ammoText.text = "";
            ammoText.enabled = false;
        }

        // Method to update the ammoText
        public void updateAmmoText()
        {
            if (slotItem != null)
            {
                //Check if the item is a weapon
                if(slotItem.CompareTag("Gun"))
                {
                    //Get the weapon stats ready
                    Weapon weaponScript = slotItem.GetComponent<Weapon>();

                    //Check if the ammo is low
                    if(weaponScript.getCurrentAmmo() <= weaponScript.weaponStats.ammoThreshold)
                    {
                        ammoText.text = $"<color=#960000>{weaponScript.getCurrentAmmo()}</color> / {weaponScript.weaponStats.maxAmmo}";
                    }
                    else 
                    {
                        ammoText.text = $"<color=#FFFFFF>{weaponScript.getCurrentAmmo()}</color> / {weaponScript.weaponStats.maxAmmo}";
                    }
                }
            }
        }

        // Getter for the slot's item
        public Item getItem()
        {
            return slotItem;
        }

        // Getter for the slot's icon
        public Image getIcon()
        {
            return slotIcon;
        }

        // Getter for the slot's ammoText
        public TMP_Text getAmmoText()
        {
            return ammoText;
        }

        //Method that shows or hides the select frame
        public void setSelectFrame(bool state)
        {
            if(state)
            {
                selectedFrame.enabled = true;
            }
            else
            {
                selectedFrame.enabled = false;
            }
        }
    }

    private PlayerController controllerScript;  // Reference to the controller script

    private InventorySlot[] inventorySlots;  // Array of inventory UI Image slots
    private int currentSlot = -1;     // The selected inventory slot (-1 to 4) where -1 represents no slot selected

    public Item startWeapon;  //Weapon that the player starts with

    private void Start()
    {   
        // Initialize inventory slots
        int slotCount = 5; // Number of slots in the inventory
        inventorySlots = new InventorySlot[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            inventorySlots[i] = new InventorySlot(i+1);
        }

        //Set the start weapon
        inventorySlots[0].setItemInSlot(startWeapon);

        controllerScript = GetComponent<PlayerController>();    //Locate controller script on player

        UpdateInventoryUI();
    }

    private void Update()
    {
        //Only call method if we actually need to
        if((Input.GetKeyDown(KeyCode.Alpha1)) || (Input.GetKeyDown(KeyCode.Alpha2)) || (Input.GetKeyDown(KeyCode.Alpha3)) || (Input.GetKeyDown(KeyCode.Alpha4)) || (Input.GetKeyDown(KeyCode.Alpha5)))
        {
            HandleInventorySelection();
            HandleItemUse();
        }
    }

    // Switch between inventory slots using keys 1 to 5
    private void HandleInventorySelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) 
        {
            if(currentSlot == 0)
            {
                currentSlot = -1;
            }
            else 
            {
                currentSlot = 0;
            } 
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if(currentSlot == 1)
            {
                currentSlot = -1;
            }
            else 
            {
                currentSlot = 1;
            } 
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if(currentSlot == 2)
            {
                currentSlot = -1;
            }
            else 
            {
                currentSlot = 2;
            } 
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if(currentSlot == 3)
            {
                currentSlot = -1;
            }
            else 
            {
                currentSlot = 3;
            } 
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            if(currentSlot == 4)
            {
                currentSlot = -1;
            }
            else 
            {
                currentSlot = 4;
            } 
        }

        // Update the UI to show which slot is currently selected
        UpdateInventoryUI();
    }

    // Update the appearance of the inventory slots to reflect selection
    private void UpdateInventoryUI()
    {
        //Loop through all slots
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];  //The current slot in the loop

            //Sort out which slot is getting highlighted
            if (i == currentSlot)
            {
                // Enable the selected border
                slot.setSelectFrame(true);
            }
            else
            {
                // Disable the selected border
                slot.setSelectFrame(false);
            }
        }
    }

    //Method to tell player's controller to set the slot as current weapon
    private void HandleItemUse()
    {
        if(currentSlot >= 0)
        {
            //Current item being held
            Item currentItem = inventorySlots[currentSlot].getItem();

            // Check if the current selected slot has an item
            if (currentItem != null)
            {
                //Set current item to item at that slot
                controllerScript.setCurrentItem(currentItem);

                // Activate the weapon in the player's hand
                Transform playerWeaponTransform = transform.Find($"Weapons/{currentItem.name}");
                if (playerWeaponTransform != null)
                {
                    GameObject playerWeapon = playerWeaponTransform.gameObject;
                    playerWeapon.SetActive(true); // Ensure the weapon is active
                }
            }
            else
            {
                controllerScript.setCurrentItem(null);
            }
        }
        else
        {
            controllerScript.setCurrentItem(null);
        }
    }

    //Getter for inventorySlots - return item from a given slot number
    public Item getItemFromSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventorySlots.Length)
        {
            return inventorySlots[slotIndex].getItem();
        }
        else
        {
            return null;
        }
    }

    // Swap weapons between two slots
    public void SwapItems(int slotIndex1, int slotIndex2)
    {
        //Check that valid slots has been chosen
        if((slotIndex1 >= 0 && slotIndex1 < inventorySlots.Length) && (slotIndex2 >= 0 && slotIndex2 < inventorySlots.Length))
        {
            Item item1 = inventorySlots[slotIndex1].getItem();
            Item item2 = inventorySlots[slotIndex2].getItem();

            // Swap weapons in the inventory array
            inventorySlots[slotIndex1].setItemInSlot(item2);
            inventorySlots[slotIndex2].setItemInSlot(item1);
        }
        //Check if the player tried to throw away a weapon by using the bin
        else if(slotIndex1 == 5 || slotIndex2 == 5)
        {
            //The slot index that wasn't 5 is the item that needs to be removed
            if(slotIndex1 == 5)
            {
                //remove the item at index2 but only if there is atleast 1 other weapon
                if(getNumberOfFilledSlots() > 1)
                {
                    removeItemFromSlot(inventorySlots[slotIndex2].getItem());
                }
                else
                {
                    Debug.LogWarning("The player must atleast carry 1 weapon");
                }
            }
            else
            {
                //remove the item at index1 but only if there is atleast 1 other weapon
                if(getNumberOfFilledSlots() > 1)
                {
                    removeItemFromSlot(inventorySlots[slotIndex1].getItem());
                }
                else
                {
                    Debug.LogWarning("The player must atleast carry 1 weapon");
                }
            }
        }
        //UpdateCurrentItem
        HandleItemUse();
    }

    //Helper method to return how many slots have items in them
    public int getNumberOfFilledSlots()
    {
        int filledSlots = 0;
        foreach(InventorySlot slot in inventorySlots)
        {
            //Check if the current slot has an item
            if(slot.getItem() != null)
            {
                filledSlots++;
            }
        }
        return filledSlots; //Should return any number from 1 to 5 as the player should never have 0 weapons
    }

    // Getter method to return the player's current inventory state
    public InventorySlot[] getInventorySlots()
    {
        return inventorySlots;
    }

    //Method to change the weapon image icon to desired format - These values were tested 1 by 1 for best looking
    public void setIconOrientation(string nameOfItem, Image slotImage)
    {
        RectTransform rectTransform = slotImage.GetComponent<RectTransform>();   //Get the transform component
        //Different values for each weapon
        if(nameOfItem == "AK47")
        {
            // Adjust scale to taste
            rectTransform.localScale = new Vector3(1.2f, 0.6f, 0);

            // Adjust rotation to taste
            rectTransform.localEulerAngles = new Vector3(0, 0, 40f);

            // Adjust position to taste
            rectTransform.anchoredPosition = new Vector2(5f, -5f);
        }
        else if(nameOfItem == "M4")
        {
            // Adjust scale to taste
            rectTransform.localScale = new Vector3(1.3f, 0.6f, 0);

            // Adjust rotation to taste
            rectTransform.localEulerAngles = new Vector3(0, 0, 40f);

            // Adjust position to taste
            rectTransform.anchoredPosition = new Vector2(7f, -7f);
        }
        else if(nameOfItem == "M249")
        {
            // Adjust scale to taste
            rectTransform.localScale = new Vector3(1.4f, 0.4f, 0);

            // Adjust rotation to taste
            rectTransform.localEulerAngles = new Vector3(0, 0, 40f);

            // Adjust position to taste
            rectTransform.anchoredPosition = new Vector2(7f, 0f);
        }
        else if(nameOfItem == "M2_50cal")
        {
            // Adjust scale to taste
            rectTransform.localScale = new Vector3(1.2f, 0.9f, 0);

            // Adjust rotation to taste
            rectTransform.localEulerAngles = new Vector3(0, 0, 20f);

            // Adjust position to taste
            rectTransform.anchoredPosition = new Vector2(0f, 2f);
        }
        else if(nameOfItem == "Uzi")
        {
            // Adjust scale to taste
            rectTransform.localScale = new Vector3(1.2f, 0.7f, 0);

            // Adjust rotation to taste
            rectTransform.localEulerAngles = new Vector3(0, 0, 30f);

            // Adjust position to taste
            rectTransform.anchoredPosition = new Vector2(6f, -4f);
        }
        else if(nameOfItem == "Benelli_M4")
        {
            // Adjust scale to taste
            rectTransform.localScale = new Vector3(1.2f, 0.4f, 0);

            // Adjust rotation to taste
            rectTransform.localEulerAngles = new Vector3(0, 0, 30f);

            // Adjust position to taste
            rectTransform.anchoredPosition = new Vector2(0f, 0f);
        }
        else if(nameOfItem == "M1911")
        {
            // Adjust scale to taste
            rectTransform.localScale = new Vector3(1.3f, 1, 0);

            // Adjust rotation to taste
            rectTransform.localEulerAngles = new Vector3(0, 0, 20f);

            // Adjust position to taste
            rectTransform.anchoredPosition = new Vector2(0f, 0f);
        }
        else if(nameOfItem == "GoldenGun")
        {
            // Adjust scale to taste
            rectTransform.localScale = new Vector3(1.1f, 0.9f, 0);

            // Adjust rotation to taste
            rectTransform.localEulerAngles = new Vector3(0, 0, 20f);

            // Adjust position to taste
            rectTransform.anchoredPosition = new Vector2(3f, 2f);
        }
    }

    //Method to add item to any given slot
    public void addItemToSlot(Item itemToAdd, int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventorySlots.Length)
        {
            // Ensure the item's initialization is complete before adding it
            Weapon weaponScript = itemToAdd.GetComponent<Weapon>();
            if (weaponScript != null && string.IsNullOrEmpty(weaponScript.getItemName()))
            {
                weaponScript.Awake(); // Force initialization if it hasn't happened yet
            }

            // Perform a recursive search for the Weapons container
            Transform weaponsContainer = FindChildWithName(transform, "Weapons");
            if (weaponsContainer != null)
            {
                // Clone the weapon to ensure uniqueness
                GameObject weaponClone = Instantiate(itemToAdd.gameObject, weaponsContainer);

                // Set the parent of the weapon clone to the Weapons container
                weaponClone.transform.SetParent(weaponsContainer);

                // Match the position and rotation of the original weapon prefab
                Transform originalWeaponTransform = weaponsContainer.Find(itemToAdd.name);
                if (originalWeaponTransform != null)
                {
                    weaponClone.transform.localPosition = originalWeaponTransform.localPosition;
                    weaponClone.transform.localRotation = originalWeaponTransform.localRotation;
                    weaponClone.transform.localScale = originalWeaponTransform.localScale; // Match scale if needed
                }
                else
                {
                    Debug.LogWarning($"Original weapon {itemToAdd.name} not found in Weapons container. Using default transform.");
                    weaponClone.transform.localPosition = Vector3.zero;
                    weaponClone.transform.localRotation = Quaternion.identity;
                }

                // Rename the clone for clarity
                weaponClone.name = $"{itemToAdd.name}_Clone_{slotIndex}";

                // Ensure the cloned weapon is inactive
                weaponClone.SetActive(false);

                // Add the cloned weapon to the inventory slot
                inventorySlots[slotIndex].setItemInSlot(weaponClone.GetComponent<Item>());
            }
            else
            {
                Debug.LogError("Weapons container not found!");
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


    // Remove item from a specific slot
    public void removeItemFromSlot(Item itemToRemove)
    {
        //Make sure there is an item to remove
        if(itemToRemove != null)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].getItem() == itemToRemove)
                {
                    inventorySlots[i].removeItemInSlot();

                    // If the removed item is the current item, clear it
                    if (controllerScript.getCurrentItem() == itemToRemove)
                    {
                        controllerScript.setCurrentItem(null);
                    }
                    return;
                }
            }
        }
    }
}

