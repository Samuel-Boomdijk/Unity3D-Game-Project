using UnityEngine;

public abstract class Item : MonoBehaviour
{
    protected string itemName;         // Name of the item
    protected Sprite itemIcon;         // Icon used in the inventory

    // Virtual method for using the item (override in child classes)
    public abstract void Use(string howToUse);

    //Method to return the item name
    public string getItemName()
    {
        return itemName;
    }
}
