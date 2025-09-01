using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon Stats")]
public class WeaponStats : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;           // Name of the weapon
    public Sprite weaponIcon;           // Icon for inventory/UI
    public GameObject projectilePrefab; // For grenades or bullets
    public WeaponType weaponType;       // Enum for weapon type

    [Header("Stats")]
    public float damage;              // Damage per shot
    public float fireRate;            // Time between shots (lower is faster) (throwForce for Grenades)
    public int maxAmmo;               // Maximum ammo capacity
    public int ammoThreshold;          // What is considered a low ammo count
    public float range;               // Weapon range (Blast Radius for Grenades)
    public float reloadTime;          // Time it takes for this weapon to reload
    public bool isAutomatic;          // Automatic or single shot
    public int burstCount;            // For burst fire (Weapons without burst are set to 0 and shotgun uses this for pellet count)

    [Header("Special Effects")]
    public GameObject muzzleFlashPrefab; // Muzzle flash effect

    public enum WeaponType
    {
        Pistol,
        SMG,
        Rifle,
        Shotgun,
        MachineGun
    }
}
