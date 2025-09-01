using UnityEngine;
using System.Collections;

public class Weapon : Item
{
    public WeaponStats weaponStats;     // Reference to the weapon stats

    protected int currentAmmo;          // Current ammo in the weapon

    private bool isFiring = false; // Tracks if the weapon is firing
    private float nextFireTime = 0f; // For fire rate control

    private bool isReloading = false;   //Is the weapon currently reloading
    private Transform firePoint;         // Reference to the fire point (where the bullets spawn)

    private GameObject player;  // Reference to the player

    // Event to notify changes in ammo
    public delegate void AmmoChangedHandler();
    public event AmmoChangedHandler OnGunAmmoChanged;

    public void Awake()
    {
        InitializeWeapon();
    }

    protected void OnEnable()
    {
        // Ensure initialization happens if the object is reactivated
        if (string.IsNullOrEmpty(itemName))
        {
            InitializeWeapon();
        }
    }

    private void InitializeWeapon()
    {
        itemName = weaponStats.weaponName;
        itemIcon = weaponStats.weaponIcon;

        player = GameObject.FindWithTag("Player");
        firePoint = transform.Find("FirePoint");
        if (firePoint == null) 
        {
            Debug.LogWarning($"Firepoint of {weaponStats.weaponName} not set");
        }

        // Ensure current ammo is initialized to max
        if (currentAmmo == 0)
        {
            currentAmmo = weaponStats.maxAmmo;
        }
    }

    // Override the Use method
    public override void Use(string howToUse)
    {
        //Check how the weapon should be used
        if(howToUse == "Shoot non-automatic gun")
        {
            Shoot();
        }
        else if(howToUse == "Shoot automatic gun")
        {
            StartFiring();
        }
        else if(howToUse == "Stop shooting automatic gun")
        {
            StopFiring();
        }
        else if(howToUse == "Reload gun")
        {
            Reload();
        }
    }

    // Shooting method (can be overridden in specific weapons if needed)
    protected virtual void Shoot()
    {
        // If currently reloading, we can't shoot
        if (!isReloading)
        {
            //If enough time has passed
            if (Time.time >= nextFireTime)
            {
                //If we also have ammo
                if(currentAmmo > 0)
                {
                    //Check if it is a burst gun
                    if (weaponStats.weaponName == "M4")
                    {
                        StartCoroutine(BurstFire());
                    }
                    else
                    {
                        Fire();
                    }
                    nextFireTime = Time.time + weaponStats.fireRate;
                }
            }
        }
    }

    //Method for automatic weapons
    public void StartFiring()
    {
        if (!isFiring)
        {
            isFiring = true;
            StartCoroutine(AutomaticFire());
        }
    }

    //Method for automatic weapons
    public void StopFiring()
    {
        isFiring = false; // Stop automatic fire
    }

    //Method for automatic weapons
    private IEnumerator AutomaticFire()
    {
        while (isFiring && weaponStats.isAutomatic)
        {
            Shoot();
            yield return new WaitForSeconds(weaponStats.fireRate);
        }
    }

    //Method for burst fire
    private IEnumerator BurstFire()
    {
        int shotsFired = 0;
        while (shotsFired < weaponStats.burstCount)
        {
            if (currentAmmo > 0)
            {
                Fire();
                shotsFired++;
                yield return new WaitForSeconds(weaponStats.fireRate);
            }
            else
            {
                break;
            }
        }
    }


    // Method to shoot gun
    private void Fire()
    {
        if (weaponStats.weaponType == WeaponStats.WeaponType.Shotgun)
        {
            FireShotgun(); // Fire multiple pellets
        }
        else
        {
            // Instantiate muzzle flash, projectile, or play sound
            if (weaponStats.muzzleFlashPrefab)
            {
                Instantiate(weaponStats.muzzleFlashPrefab, transform.position, transform.rotation);
            }

            //Shoot the gun
            RaycastHit hit;
            if(firePoint == null) {Debug.LogWarning("Firepoint of weapon not set at runtime");}
            Vector3 firePointPosition = firePoint.position;
            Vector3 fireDirection = player.transform.forward;

            // Perform a raycast to detect the hit
            if (Physics.Raycast(firePointPosition, fireDirection, out hit, weaponStats.range))
            {
                // Hit detected, Deal damage to the hit object if it has a health script
                EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(weaponStats.damage);
                }

                // Create the bullet ray effect
                CreateBulletRay(firePointPosition, hit.point);
            }
            else
            {
                // No hit detected, shoot the ray into empty space
                Vector3 endPoint = firePointPosition + (fireDirection * weaponStats.range);
                CreateBulletRay(firePointPosition, endPoint);
            }

            currentAmmo--; // Reduce ammo
            if (currentAmmo < 0) {currentAmmo = 0;} //Make sure ammo doesn't fall below 0
            // Notify listeners that the ammo count has changed
            OnGunAmmoChanged?.Invoke();
        }
    }

    //Method to create the ray
    private void CreateBulletRay(Vector3 startPoint, Vector3 endPoint)
    {
        // Instantiate the bullet ray prefab
        GameObject bullet = Instantiate(weaponStats.projectilePrefab, startPoint, Quaternion.identity);

        // Pass the start and end points to the Line Renderer
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.FireRay(startPoint, endPoint);
        }
        else
        {
            Debug.LogError("Bullet script is missing on the bullet ray prefab!");
        }
    }

    private void FireShotgun()
    {
        int pelletCount = weaponStats.burstCount; // Number of pellets per shot
        float spreadAngle = 25f; // Spread angle of the shotgun

        for (int i = 0; i < pelletCount; i++)
        {
            // Randomize the spread direction
            Quaternion spreadRotation = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                0
            );

            Vector3 spreadDirection = spreadRotation * player.transform.forward;

            // Perform a raycast for each pellet
            RaycastHit hit;
            if (Physics.Raycast(firePoint.position, spreadDirection, out hit, weaponStats.range))
            {
                // Hit detected, Deal damage to the hit object
                EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(weaponStats.damage);
                }

                // Create a visual ray for the pellet
                CreateBulletRay(firePoint.position, hit.point);
            }
            else
            {
                // No hit, create a ray into empty space
                Vector3 endPoint = firePoint.position + (spreadDirection * weaponStats.range);
                CreateBulletRay(firePoint.position, endPoint);
            }
        }
        currentAmmo--; // Reduce ammo
        OnGunAmmoChanged?.Invoke();
    }



    // Reload method (can be overridden in specific weapons if needed)
    protected virtual void Reload()
    {
        if (isReloading)
        {
            // Prevent reloading if already reloading
            return;
        }
        else if(currentAmmo == weaponStats.maxAmmo)
        {
            // Prevent reloading if weapon is full
            return;
        }

        isReloading = true;

        // Simulate reload time
        Invoke(nameof(FinishReload), weaponStats.reloadTime); // 2-second reload time
    }

    private void FinishReload()
    {
        currentAmmo = weaponStats.maxAmmo;
        isReloading = false;
        // Notify listeners that the ammo count has changed
        OnGunAmmoChanged?.Invoke();
    }

    //Getter for currentAmmo
    public int getCurrentAmmo()
    {
        return currentAmmo;
    }
}
