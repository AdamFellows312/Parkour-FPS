using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashing : MonoBehaviour
{
    private float dashCooldownTimer;

    private Rigidbody _rigidbody;

    private Vector3 delayedDashForce;

    [SerializeField] private bool multidirectional = true;
    [SerializeField] private bool disableGravity = false;
    [SerializeField] private bool resetVelocity = true;
    [Space(10)]

    [SerializeField] private float dashForce = 20.0f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashCooldown = 1.5f;
    [Space(10)]

    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerCamera;

    // Called on the first frame before Update()
    private void Start()
    {
        // Get the player's Rigidbody
        _rigidbody = PlayerMovement.instance.GetRigidbody();
        playerCamera = PlayerCamera.instance.transform.parent.parent;
    }

    // Called every frame the application is running
    private void Update()
    {
        GetInput();

        // Decrease the dash cooldown timer
        if (dashCooldownTimer > 0.0f) { dashCooldownTimer -= Time.deltaTime; }
    }

    private void GetInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Dash();
        }
    }

    private void Dash()
    {
        // Don't dash if the timer hsn't reached zero (still decreasing)
        if (dashCooldownTimer > 0.0f) { return; }
        // Reset the dash timer
        else { dashCooldownTimer = dashCooldown; }

        PlayerMovement.instance.isDashing = true;

        // Update the cameras FOV
        PlayerCamera.instance.CameraFovChange(90.0f);

        // Determine which forward transform to use
        Transform forwardTransform;
        //if (useCameraForward) { forwardTransform = playerCamera; }
        //else { forwardTransform = orientation; }
        forwardTransform = orientation;

        Vector3 direction = GetDirection(forwardTransform);
        // Calculate the force to apply when dashing
        Vector3 appliedForce = (direction * dashForce) /*+ (orientation.up * dashUpwardForce)*/;

        // Disable the gravity if required
        if (disableGravity) { Debug.Log("Disable rigidbody gavity"); _rigidbody.useGravity = false; }

        // Set the dash force to the applied force
        delayedDashForce = appliedForce;
        Invoke(nameof(DelayedDashForce), 0.025f);

        // Reset the dash after the dash is finished
        Invoke(nameof(ResetDash), dashDuration);
    }

    private void DelayedDashForce()
    {
        // Reset velocity before dashing if required
        if (resetVelocity) { _rigidbody.velocity = Vector3.zero; }
        // Apply the delayed dash force
        _rigidbody.AddForce(delayedDashForce, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        // Reset the is dashing boolean
        PlayerMovement.instance.isDashing = false;
        // Re-enable the gravity if required
        if (disableGravity) { _rigidbody.useGravity = true; }

        // Reset the camera FOV
        PlayerCamera.instance.CameraFovChange(PlayerCamera.instance.originalFOV);
    }

    private Vector3 GetDirection(Transform forwardTransform)
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3();

        // Calculate the direction based on the forward transform and the input if multidirectional
        if (multidirectional) { direction = (forwardTransform.forward * v) + (forwardTransform.right * h); }
        // Only allow dashing in the forward direction of the forward transform;
        else { direction = forwardTransform.forward; }

        // Default to the forward direction if no input is being pressed
        if (h == 0.0f && v == 0.0f) { direction = forwardTransform.forward; }

        direction.Normalize();
        return direction;
    }
}
