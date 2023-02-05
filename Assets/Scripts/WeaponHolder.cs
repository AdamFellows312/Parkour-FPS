using System;
using System.Collections;
using UnityEngine;

//@TODO: Comment this all later
public class WeaponHolder : MonoBehaviour
{
    public static WeaponHolder instance;

    private bool isReloading = false;

    private int spins;

    private float gunDrag = 0.2f;

    private float destinationX;
    private float destinationY;

    private float reloadRotation;
    private float reloadPositionOffset;
    private float desiredReloadRotation;
    private float reloadProgress;
    private float reloadTime;
    private float reloadPositionVelocity;

    private float xBobSpeed = 0.12f;
    private float yBobSpeed = 0.08f;
    private float zBobSpeed = 0.1f;
    private float bobSpeedMultiplier = 0.45f;

    private Rigidbody _rigidbody;

    private Vector3 recoilRotation;
    private Vector3 recoilOffset;
    private Vector3 recoilOffsetVelocity;
    private Vector3 recoilRotationVelocity;

    private Vector3 desiredBob;
    private Vector3 startPosition;

    [SerializeField] private float reloadSpinDuration = 0.45f;
    [SerializeField] private float currentGunDragMultiplier = 0.15f;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        // Get the plaayer's rigidbody
        _rigidbody = PlayerMovement.instance.GetRigidbody();
        startPosition = transform.localPosition;
    }

    private void Update()
    {
        MovementBob();
        ReloadGun();
        RecoilGun();
        WeaponDrag();

        //if (Input.GetKeyDown(KeyCode.Mouse0)) { Recoil(); }

        Vector3 updatedPosition = startPosition + new Vector3(destinationX, destinationY, 0.0f) + desiredBob + recoilOffset +
            new Vector3(0.0f, 0.0f - reloadPositionOffset, 0.0f);
        transform.localPosition = Vector3.Lerp(transform.localPosition, updatedPosition, Time.unscaledDeltaTime * 15f);
    }

    private void WeaponDrag()
    {
        float dragX = 0.0f;
        float dragY = 0.0f;

        // Get the drag on the X and Y axis, subtracting from zero to move in opposite direction to mouse
        dragX = (0f - Input.GetAxis("Mouse X")) * gunDrag * currentGunDragMultiplier;
        dragY = (0f - Input.GetAxis("Mouse Y")) * gunDrag * currentGunDragMultiplier;

        // Get the destintion of the holder on the X and Y axis
        destinationX = Mathf.Lerp(destinationX, dragX, Time.unscaledDeltaTime * 10f);
        destinationY = Mathf.Lerp(destinationY, dragY, Time.unscaledDeltaTime * 10f);
        Vector2 destinationVector = new Vector2(destinationX, destinationY);

        // Perform the rotation
        Rotation(destinationVector);
    }

    private void Rotation(Vector2 offset)
    {
        float affectedOffset = offset.magnitude * 0.03f;
        if (offset.x < 0.0f)
        {
            affectedOffset = 0.0f - affectedOffset;
        }

        float offsetY = offset.y;
        // Get the rotation euler based off the offset (inputed vector rotation)
        Vector3 euler = new Vector3(y: (0.0f - offset.x) * 40.0f, x: offsetY * 80.0f + reloadRotation, z: affectedOffset * 50.0f) + recoilRotation;
        try
        {
            if (!(Time.deltaTime <= 0.0f))
            {
                // Perform the rotation
                 transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(euler), Time.deltaTime * 20f);
            }
        }
        catch (Exception) { }
    } 

    private void MovementBob()
    {
        if (_rigidbody)
        {
            // Don't bob if the player isn't moving / is airborne / is crouching / is sliding
            if (Mathf.Abs(_rigidbody.velocity.magnitude) < 4.0f || !PlayerMovement.instance.IsGrounded() || PlayerMovement.instance.isCrouching || PlayerMovement.instance.isSliding)
            {
                desiredBob = Vector3.zero;
                return;
            }
            // Calculate the bob on each axis
            float xBob = Mathf.PingPong(Time.time * bobSpeedMultiplier, xBobSpeed) - xBobSpeed / 2f;
            float yBob = Mathf.PingPong(Time.time * bobSpeedMultiplier, yBobSpeed) - yBobSpeed / 2f;
            float zBob = Mathf.PingPong(Time.time * bobSpeedMultiplier, zBobSpeed) - zBobSpeed / 2f;
            // Update the desired bob
            desiredBob = new Vector3(xBob, yBob, zBob);
        }
    }

    private void RecoilGun()
    {
        Mathf.Clamp(recoilOffset.z, 100.0f, -0.425f);
        recoilOffset = Vector3.SmoothDamp(recoilOffset, Vector3.zero, ref recoilOffsetVelocity, 0.05f);
        recoilRotation = Vector3.SmoothDamp(recoilRotation, Vector3.zero, ref recoilRotationVelocity, 0.07f);
    }

    public void Recoil()
    {
        // Using UnityEngine.Random to remove ambiguity between UnityEngine.Random and System.Random
        recoilOffset.z -= UnityEngine.Random.Range(1.75f, 3.0f); // Placeholder valuesr
        //recoilRotation.x -= UnityEngine.Random.Range(22.5f, 35.75f) * 1.5f; // Placeholder values
    }

    private void ReloadGun()
    {
        reloadProgress += Time.deltaTime;
        reloadRotation = Mathf.Lerp(0f, desiredReloadRotation, reloadProgress / reloadTime);
        reloadPositionOffset = Mathf.SmoothDamp(reloadPositionOffset, 0f, ref reloadPositionVelocity, reloadTime * 0.2f);

        if (reloadRotation / 360f > (float)spins)
        {
            spins++;
        }
    }

    public void TryReload(float reloadDuration)
    {
        if (isReloading) return;

        Debug.Log("Reload");

        isReloading = true;
        StartCoroutine(Reload(360.0f, reloadDuration));
    }

    private IEnumerator Reload(float targetValue, float duration)
    {
        float time = 0;
        float startValue = 0.0f;
        while (time < duration)
        {
            desiredReloadRotation = Mathf.Lerp(startValue, targetValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        desiredReloadRotation = targetValue;
        isReloading = false;
    }
}
