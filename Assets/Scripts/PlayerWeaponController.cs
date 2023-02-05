using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    private Transform playerCameraTransform;

    private Weapon playerWeapon;

    [SerializeField] private LayerMask hitMask;

    private void Start()
    {
        // Get the player camera transform
        playerCameraTransform = PlayerCamera.instance.transform;
        // Get the player's current weapon 
        playerWeapon = GameObject.FindObjectOfType<Weapon>();
    }

    private void Update()
    {
        GetInput();
    }

    private void GetInput()
    {
        playerWeapon.isShooting = Input.GetMouseButtonDown(0);
    }
}
