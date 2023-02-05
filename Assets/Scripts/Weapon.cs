using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Weapon : MonoBehaviour
{
    private bool readyToUse;
    private bool isReloading;
    [HideInInspector] public bool isShooting;
    
    private int bulletsLeft;
    private int bulletsShot;

    private Transform attackPoint;
    private Transform playerCameraTransform;

    [SerializeField] private int clipSize;
    [SerializeField] private int bulletsPerShot;
    [Space(10)]
    
    // Projectile forces
    [SerializeField] private float shootForce;
    [SerializeField] private float upwardForce;
    [Space(10)]
    
    // Weapon stats
    [SerializeField] private float shootCooldown;
    [SerializeField] private float spread;
    [SerializeField] private float reloadTime;
    [SerializeField] private float betweenShotsCooldown;
    [Space(10)]

    [SerializeField] private TextMeshProUGUI ammoCount;
    [SerializeField] private GameObject projectilePrefab;
 
    private void Start()
    {
        // Get the attack point
        attackPoint = transform.GetChild(0);
        // Get the player camera's transform
        playerCameraTransform = PlayerCamera.instance.transform;

        // Assigne gun variables
        bulletsLeft = clipSize;
        // Reset shooting
        ResetUse();
    }

    private void Update()
    {
        GunBehaviour();

        if (bulletsLeft <= 0.0f && !isReloading) { StartReload(); }
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < clipSize && !isReloading) { StartReload(); }
        ammoCount.text = bulletsLeft.ToString();
    }

    private void GunBehaviour()
    {
        if (readyToUse && isShooting && !isReloading)
        {
            // Ensure the plyer still has bullets in their gun
            if (bulletsLeft > 0.0f)
            {
                bulletsShot = 0;
                // Shoot the gun
                Shoot();
            }
        }
    }

    private void Shoot()
    {
        // Don't shoot if not ready to use
        if (!readyToUse) { return; }

        readyToUse = false;

        // Get the target point
        Vector3 targetPoint = HitPoint();
        // Calculate the direction from attack point to target point without spread
        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        // Get the random spread
        float xSpread = Random.Range(-spread, spread);
        float ySpread = Random.Range(-spread, spread);
        float zSpread = Random.Range(-spread, spread);

        // Calculate the direction with spread
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(xSpread, ySpread, zSpread);

        // Actually spawn a bullet
        GameObject currentProjectile = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
        currentProjectile.transform.forward = directionWithSpread.normalized;
        // Change the colour of the gun
        ProjectileBullet projectile = currentProjectile.GetComponent<ProjectileBullet>();
        Color projectileColour = IsPlayerOwned() ? Color.blue : Color.red;
        projectile.SetBullet(projectileColour);
        // Apply a force to move the bullet
        Rigidbody currentProjectileRigidbody = currentProjectile.GetComponent<Rigidbody>();
        currentProjectileRigidbody.AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
        currentProjectileRigidbody.AddForce(playerCameraTransform.up * upwardForce, ForceMode.Impulse);

        bulletsShot++;
        bulletsLeft--;

        Invoke(nameof(ResetUse), shootCooldown);
    }

    private void StartReload()
    {
        isReloading = true;
        WeaponHolder.instance.TryReload(reloadTime);
        Invoke(nameof(EndReload), reloadTime);
    }

    private void EndReload()
    {
        isReloading = false;
        // Reset how many bullets are left in this gun
        bulletsLeft = clipSize;
    }

    private Vector3 HitPoint()
    {
        RaycastHit hit;
        Ray ray = playerCameraTransform.GetComponent<Camera>().ViewportPointToRay(Vector3.one * 0.5f);

        if (Physics.Raycast(ray, out hit)) { return hit.point; }
        else { return ray.GetPoint(5f); }
    }

    private void ResetUse() => readyToUse = true;

    private bool IsPlayerOwned()
    {
        if (transform.root.tag != "Enemy")
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
