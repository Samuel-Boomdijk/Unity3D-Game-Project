using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    private GameObject pauseMenuCanvas; // Reference to the pause menu canvas
    private GameObject playerHUD; // Reference to the playerHUD
    private GameObject player; // Reference to the player
    private GameObject InventoryMenuCanvas;  //Reference to the inventory menu canvas
    private GameObject MainMenuCanvas;  //Reference to the inventory menu canvas
    public TMP_Text pauseMenuScoreText; // Assign the PauseMenuScoreText UI element in the inspector
    public GameObject weaponMenuTextPanel; // Assign the weapon menu text UI element in the inspector

    public WeaponStats[] weaponStatsList;     // Reference to the weapon stats

    private bool isPaused = false;
    private bool canPause;

    private int lastSlotSelected = -1;

    private GameObject healthBar;
    private GameObject homePanel;
    private GameObject instructionPanel;
    private GameObject weaponPanel;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player");

        if(player != null)
        {
            pauseMenuCanvas = GameObject.FindWithTag("pauseUI");
            playerHUD = GameObject.FindWithTag("playerUI");
            InventoryMenuCanvas = GameObject.FindWithTag("playerUI");
            InventoryMenuCanvas = GameObject.FindWithTag("inventoryUI");

            healthBar = playerHUD.transform.Find("HealthBar").gameObject;

            pauseMenuCanvas.GetComponent<Canvas>().enabled = false;
            InventoryMenuCanvas.GetComponent<Canvas>().enabled = false;
            canPause = true;
        }
        else
        {
            canPause = false;

            //Game startUp
            MainMenuCanvas = GameObject.FindWithTag("mainCanvas");
            homePanel = MainMenuCanvas.transform.Find("homePanel").gameObject;
            instructionPanel = MainMenuCanvas.transform.Find("InstructionsPanel").gameObject;
            weaponPanel = MainMenuCanvas.transform.Find("WeaponsPanel").gameObject;
            homePanel.SetActive(true);
            instructionPanel.SetActive(false);
            weaponPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Toggle pause menu on pressing "P"
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    // Toggles the pause state
    public void TogglePause()
    {
        if(canPause)
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                if (ScoreManager.instance != null)
                {
                    // Update the score text
                    pauseMenuScoreText.text = $"Player Score: {ScoreManager.instance.GetScore()}";
                }

                // Show the pause menu and stop the game
                pauseMenuCanvas.GetComponent<Canvas>().enabled = true;
                Time.timeScale = 0; // Stop time

                // Hide playerHUD
                playerHUD.GetComponent<Canvas>().enabled = false;

                // Disable player controls
                player.GetComponent<PlayerController>().enabled = false;
                player.GetComponent<PlayerMovement>().enabled = false;

                //Set healthbar inactive
                healthBar.SetActive(false);
            }
            else
            {
                // Hide the pause menu and the inventory menu and resume the game
                pauseMenuCanvas.GetComponent<Canvas>().enabled = false;
                InventoryMenuCanvas.GetComponent<Canvas>().enabled = false;
                Time.timeScale = 1; // Resume time

                //Renable player controls
                player.GetComponent<PlayerController>().enabled = true;
                player.GetComponent<PlayerMovement>().enabled = true;

                // show playerHUD
                playerHUD.GetComponent<Canvas>().enabled = true;

                //Set healthbar active
                healthBar.SetActive(true);
            }
        }
    }

    public void playGame()
    {
        // Load the Game Scene
        SceneManager.LoadScene("startScene");
    }

    public void showInstructions()
    {
        //Start showing instructions
        homePanel.SetActive(false);
        instructionPanel.SetActive(true);
    }

    public void backToHomeScreen()
    {
        //Start showing homescreen
        homePanel.SetActive(true);
        instructionPanel.SetActive(false);
    }

    public void showWeaponScreen()
    {
        //Start showing weaponscreen
        instructionPanel.SetActive(false);
        weaponPanel.SetActive(true);
    }

    public void backToInstructionScreen()
    {
        //Start showing instructionsscreen again
        instructionPanel.SetActive(true);
        weaponPanel.SetActive(false);
    }
    
    public void showCredits()
    {
        // Code to drop down Credits
        Debug.Log("Credits dropping!");
    }

    public void quitGame()
    {
        // Quit the game
        Debug.Log("Game Quit!");
        Application.Quit();
    }

    public void goToMenuScreen()
    {
        //Unpause the game
        Time.timeScale = 1f;  // Unpause the game
        
        // Load the Game Scene
        SceneManager.LoadScene("MainMenu");
    }

    // Inventory screen
    public void SortInventory()
    {
        //Setup all the buttons according to the current state of the player inventory
        setupButtons();

        // Show the inventory menu and hide the pause menu 
        InventoryMenuCanvas.GetComponent<Canvas>().enabled = true;
        pauseMenuCanvas.GetComponent<Canvas>().enabled = false;
    }
    
    // Method to fill the buttons with the current PlayerUI state
    public void setupButtons()
    {
        //Reference all the buttons and add them to an array to handle all together
        GameObject panel = InventoryMenuCanvas.transform.Find("backgroundPanel").gameObject;
        Button slot1 = panel.transform.Find("slot1").GetComponent<Button>();
        Button slot2 = panel.transform.Find("slot2").GetComponent<Button>();
        Button slot3 = panel.transform.Find("slot3").GetComponent<Button>();
        Button slot4 = panel.transform.Find("slot4").GetComponent<Button>();
        Button slot5 = panel.transform.Find("slot5").GetComponent<Button>();
        Button[] slotButtons = {slot1, slot2, slot3, slot4, slot5};

        //Get the current state of the player's inventory
        PlayerInventory.InventorySlot[] playerInventorySlots = player.GetComponent<PlayerInventory>().getInventorySlots();

        //Loop through all the buttons
        for(int i = 0; i < slotButtons.Length; i++)
        {
            //Get the button's icon and text
            Image slotIcon = slotButtons[i].transform.Find("Icon").GetComponent<Image>();
            TMP_Text slotText = slotButtons[i].transform.Find("ammoText").GetComponent<TMP_Text>();

            //Check if there is an item in that slot
            if(playerInventorySlots[i].getItem() != null)
            {
                //Set the button's icon and text to the weapons' in that slot
                slotIcon.sprite = playerInventorySlots[i].getIcon().sprite;
                slotText.text = playerInventorySlots[i].getAmmoText().text;
                
                //Check what for sort item it is
                if(playerInventorySlots[i].getItem().CompareTag("Gun"))
                {
                    //If the item is a Gun
                    Weapon weaponScript = playerInventorySlots[i].getItem().GetComponent<Weapon>();
                    player.GetComponent<PlayerInventory>().setIconOrientation(weaponScript.weaponStats.weaponName, slotIcon);
                }
                slotIcon.enabled = true;
                slotText.enabled = true;
            }
            else
            {
                //Set the button's icon and text to nothing
                slotIcon.sprite = null;
                slotText.text = null;

                slotIcon.enabled = false;
                slotText.enabled = false;
            }
        }
    }

    // Called when a player selects a slot from the InventoryUI (via button press)
    public void OnSlotSelected(int slotIndex)
    {
        // If lastSlotSelected is -1, it is the first click
        if (lastSlotSelected == -1)
        {
            lastSlotSelected = slotIndex;
        }
        else
        {
            // Will only run on the second click, check if the two options are different
            if(lastSlotSelected != slotIndex)
            {
                // Swap slots
                player.GetComponent<PlayerInventory>().SwapItems(lastSlotSelected, slotIndex);
            }

            //Reset lastSlotSelected
            lastSlotSelected = -1;
        }

        //Get the current state of the player's inventory
        setupButtons();
    }

    // Called when a player selects a weapon from the Weapon instructions menu (via button press)
    public void OnWeaponSelected(int slotIndex)
    {
        TMP_Text nameText = weaponMenuTextPanel.transform.Find("NameText").GetComponent<TMP_Text>();
        TMP_Text typeText = weaponMenuTextPanel.transform.Find("TypeText").GetComponent<TMP_Text>();
        TMP_Text damageText = weaponMenuTextPanel.transform.Find("DamageText").GetComponent<TMP_Text>();
        TMP_Text firingRateText = weaponMenuTextPanel.transform.Find("FiringRateText").GetComponent<TMP_Text>();
        TMP_Text firingTypeText = weaponMenuTextPanel.transform.Find("FiringTypeText").GetComponent<TMP_Text>();
        TMP_Text ammoSizeText = weaponMenuTextPanel.transform.Find("AmmoSizeText").GetComponent<TMP_Text>(); 
        TMP_Text rangeText = weaponMenuTextPanel.transform.Find("RangeText").GetComponent<TMP_Text>(); 
        TMP_Text reloadTimeText = weaponMenuTextPanel.transform.Find("ReloadTimeText").GetComponent<TMP_Text>();
        TMP_Text dropRateText = weaponMenuTextPanel.transform.Find("Drop%Text").GetComponent<TMP_Text>();
        TMP_Text notesText = weaponMenuTextPanel.transform.Find("NotesText").GetComponent<TMP_Text>();

        //Default values
        string name = "M1911";
        WeaponStats.WeaponType weaponType = WeaponStats.WeaponType.Pistol;
        float damage = 20f;
        float firingRate = 0.3f;
        bool isAutomatic = false;
        int ammoSize = 15;
        float range = 15f;
        float reloadTime = 1f;
        string dropRate = "25%";
        string finalNotes = "";

        //Check which guns is selected
        if (slotIndex == 1)
        {    
            //Gun is pistol
            WeaponStats weapon = findWeaponStatsByName("M1911");
            if(weapon != null)
            {
                name = weapon.weaponName;
                weaponType = weapon.weaponType;
                damage = weapon.damage;
                firingRate = weapon.fireRate;
                isAutomatic = weapon.isAutomatic;
                ammoSize = weapon.maxAmmo;
                range = weapon.range;
                reloadTime = weapon.reloadTime;
                dropRate = "25%";
                finalNotes = $"Common {weaponType} - Default weapon";
            }
        }
        else if(slotIndex == 2)
        {
            //Gun is uzi
            WeaponStats weapon = findWeaponStatsByName("Uzi");
            if(weapon != null)
            {
                name = weapon.weaponName;
                weaponType = weapon.weaponType;
                damage = weapon.damage;
                firingRate = weapon.fireRate;
                isAutomatic = weapon.isAutomatic;
                ammoSize = weapon.maxAmmo;
                range = weapon.range;
                reloadTime = weapon.reloadTime;
                dropRate = "22%";
                finalNotes = $"Light weight {weaponType} - great balance of damage and mobility ";
            }   
        }
        else if(slotIndex == 3)
        {
            //Gun is uzi
            WeaponStats weapon = findWeaponStatsByName("M4");
            if(weapon != null)
            {
                name = weapon.weaponName;
                weaponType = weapon.weaponType;
                damage = weapon.damage;
                firingRate = weapon.fireRate;
                isAutomatic = weapon.isAutomatic;
                ammoSize = weapon.maxAmmo;
                range = weapon.range;
                reloadTime = weapon.reloadTime;
                dropRate = "20%";
                finalNotes = "Basic Assualt Rifle - shoots in bursts of 3";
            }   
        }
        else if(slotIndex == 4)
        {
            //Gun is uzi
            WeaponStats weapon = findWeaponStatsByName("Benelli_M4");
            if(weapon != null)
            {
                name = weapon.weaponName;
                weaponType = weapon.weaponType;
                damage = weapon.damage;
                firingRate = weapon.fireRate;
                isAutomatic = weapon.isAutomatic;
                ammoSize = weapon.maxAmmo;
                range = weapon.range;
                reloadTime = weapon.reloadTime;
                dropRate = "15%";
                finalNotes = $"Advanced {weaponType} - Great for dealing with large hordes of enemies";
            }   
        }
        else if(slotIndex == 5)
        {
            //Gun is uzi
            WeaponStats weapon = findWeaponStatsByName("AK47");
            if(weapon != null)
            {
                name = weapon.weaponName;
                weaponType = weapon.weaponType;
                damage = weapon.damage;
                firingRate = weapon.fireRate;
                isAutomatic = weapon.isAutomatic;
                ammoSize = weapon.maxAmmo;
                range = weapon.range;
                reloadTime = weapon.reloadTime;
                dropRate = "12%";
                finalNotes = $"Premium Assault {weaponType} - Fast and deadly";
            }   
        }
        else if(slotIndex == 6)
        {
            //Gun is uzi
            WeaponStats weapon = findWeaponStatsByName("M249");
            if(weapon != null)
            {
                name = weapon.weaponName;
                weaponType = weapon.weaponType;
                damage = weapon.damage;
                firingRate = weapon.fireRate;
                isAutomatic = weapon.isAutomatic;
                ammoSize = weapon.maxAmmo;
                range = weapon.range;
                reloadTime = weapon.reloadTime;
                dropRate = "5%";
                finalNotes = $"{weaponType} - Beware, this weapon is very heavy, not suitable for kids";
            }   
        }
        else if(slotIndex == 7)
        {
            //Gun is uzi
            WeaponStats weapon = findWeaponStatsByName("GoldenGun");
            if(weapon != null)
            {
                name = weapon.weaponName;
                weaponType = weapon.weaponType;
                damage = weapon.damage;
                firingRate = weapon.fireRate;
                isAutomatic = weapon.isAutomatic;
                ammoSize = weapon.maxAmmo;
                range = weapon.range;
                reloadTime = weapon.reloadTime;
                dropRate = "1%";
                finalNotes = $"The One and Only Golden Gun - able to 1 shot even the toughest of opponents";
            }   
        }

        //Set the values
        nameText.text = $"Name: <color=#960000>{name}</color>";
        typeText.text = $"Weapon Type: <color=#960000>{weaponType}</color>";
        if(name == "GoldenGun")
        {
            damageText.text = $"Damage: <color=#960000>XXX</color>";
        }
        else
        {
            damageText.text = $"Damage: <color=#960000>{damage}</color>";
        }
        firingRateText.text = $"Firing Rate: <color=#960000>{firingRate}</color>";
        if(isAutomatic)
        {
            firingTypeText.text = "Firing Type: <color=#960000>Automatic</color>";
        }
        else
        {
            firingTypeText.text = "Firing Type: <color=#960000>Semi-Automatic</color>";
        }
        ammoSizeText.text = $"Ammo Size: <color=#960000>{ammoSize}</color>";
        rangeText.text = $"Range: <color=#960000>{range}</color>";
        reloadTimeText.text = $"Reload Time: <color=#960000>{reloadTime}s</color>";
        dropRateText.text = $"Drop Rate: <color=#960000>{dropRate}</color>";
        notesText.text = $"Description: <color=#960000>{finalNotes}</color>";
    }

    //Helper Method to find the game weapon
    private WeaponStats findWeaponStatsByName(string name)
    {
        //Loop through the list of weapons
        foreach(WeaponStats weaponObject in weaponStatsList)
        {
            //Check if it is the one we're looking for
            if(weaponObject.weaponName == name)
            {
                return weaponObject;
            }
        }
        return null;    //Only if the weapon name is not in the list of weapons
    }
}
