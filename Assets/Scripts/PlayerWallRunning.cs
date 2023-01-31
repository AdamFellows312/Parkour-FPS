using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallRunning : MonoBehaviour
{
    private bool isWallLeft;
    private bool isWallRight;
    private bool isExitingWall;

    private float horizontalInput;
    private float verticalInput;
    private float exitWallTimer;
    private float wallRunTimer;

    private Rigidbody _rigidbody;

    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;

    [SerializeField] private bool useGravity = true;
    [Space(10)]

    [SerializeField] private float wallRunForce = 200.0f;
    [SerializeField] private float wallJumpUpForce = 7.0f;
    [SerializeField] private float wallJumpSideForce = 12.0f;
    [SerializeField] private float maxWallRunTime = 0.7f;
    [SerializeField] private float wallCheckDistance = 0.7f;
    [SerializeField] private float minJumpHeight = 2.0f;
    [SerializeField] private float exitWallCooldown = 0.2f;
    [SerializeField] private float gravityCounterForce = 10.0f;
    [Space(10)]

    [SerializeField] private Transform orientation;
    [Space(10)]

    [SerializeField] private LayerMask wallMask;
    [SerializeField] private LayerMask groundMask;

    private void Start()
    {
        // Get the player's rigidbody
        _rigidbody = PlayerMovement.instance.GetRigidbody();
        // Set the ground mask to the players ground mask
        groundMask = PlayerMovement.instance.groundMask;
    }

    // Called every frame the application is running
    private void Update()
    {
        CheckForWall();
        GetInput();
        PseudoStateHandler();
    }

    // Called every fixed framerate update 
    private void FixedUpdate()
    {
        if (PlayerMovement.instance.isWallrunnning)
        {
            WallRunnningMovement();
        }
    }

    private void CheckForWall()
    {
        // Check for a wall to the right of the player
        isWallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, wallMask);
        // Check for a wall to the negative right (left) of the player
        isWallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, wallMask);
    }

    private void GetInput()
    {
        // Get WASD and arrow keys input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void PseudoStateHandler()
    {
        // Wallrunning state
        if ((isWallLeft || isWallRight) && verticalInput > 0 && AboveGroundThreshold() && !isExitingWall)
        {
            Debug.Log("Try enter wall run state");
            // Initiate the wall run
            if (!PlayerMovement.instance.isWallrunnning)
            {
                StartWallRun();
            }

            // Decrese the wall run timer
            if (wallRunTimer > 0.0f)
            {
                wallRunTimer -= Time.deltaTime;
            }
            if (wallRunTimer <= 0.0f)
            {
                isExitingWall = true;
                // Reset the exit wall cooldown
                exitWallTimer = exitWallCooldown;
            }

            // Wall jump whilst in the wall running state
            if (Input.GetKeyDown(KeyCode.Space))
            {
                WallJump();
            }
        }
        // Exiting a wall state
        else if (isExitingWall)
        {
            // Cancel out of any currently active wall runs
            if (PlayerMovement.instance.isWallrunnning)
            {
                EndWallRun();
            }

            // Decrease the exit wall timer
            if (exitWallTimer > 0.0f)
            {
                exitWallTimer -= Time.deltaTime;
            }
            if (exitWallTimer <= 0.0f)
            {
                isExitingWall = false;
            }
        }
        // No state (relevant to wall running)
        else
        {
            Debug.Log("Try exit wall run state");
            if (PlayerMovement.instance.isWallrunnning)
            {
                // End the wall run
                EndWallRun();
            }
        }
    }

    private void StartWallRun()
    {
        PlayerMovement.instance.isWallrunnning = true;
        // Reset the wall run timer
        wallRunTimer = maxWallRunTime;

        // Reset the rigibody's y velocity
        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0.0f, _rigidbody.velocity.z);

        // Apply FOV and tilt effects to the camera
        PlayerCamera.instance.CameraFovChange(90.0f);
        PlayerCamera.instance.CameraTilt(isWallLeft ? -5.0f : 5.0f);
    }

    private void EndWallRun()
    {
        PlayerMovement.instance.isWallrunnning = false;

        // Redset camera FOV and tilt effects
        PlayerCamera.instance.CameraFovChange(PlayerCamera.instance.originalFOV);
        PlayerCamera.instance.CameraTilt(0.0f);
    }

    private void WallRunnningMovement()
    {
        _rigidbody.useGravity = useGravity;

        // Get the normal direction of the target wall
        Vector3 targetWallNormal = isWallRight ? rightWallHit.normal : leftWallHit.normal;
        // Get the forward direction of the target wall
        Vector3 targetWallForward = Vector3.Cross(targetWallNormal, transform.up);

        // Ensure the player doesn't wall run backwards
        if ((orientation.forward - targetWallForward).magnitude > (orientation.forward - -targetWallForward).magnitude)
        {
            targetWallForward = -targetWallForward;
        }

        // Apply the force to run along the wall
        _rigidbody.AddForce(targetWallForward * wallRunForce, ForceMode.Force);
        // Add a force to push the player into the wall to have them stick to said wall
        if (!(isWallLeft && horizontalInput > 0.0f) && !(isWallRight && horizontalInput < 0.0f))
            _rigidbody.AddForce(-targetWallNormal * 100.0f, ForceMode.Force);

        // Weaken gravity by applying a counter force in the up direction
        if (useGravity) { _rigidbody.AddForce(transform.up * gravityCounterForce, ForceMode.Force); }
    }

    private void WallJump()
    {
        Debug.Log("Wall jump");

        isExitingWall = true;
        // Reset the exit wall cooldown timer
        exitWallTimer = exitWallCooldown;

        // Get the normal direction of the target wall
        Vector3 targetWallNormal = isWallRight ? rightWallHit.normal : leftWallHit.normal;
        // Calculate the jumping force
        Vector3 forceToApply = (transform.up * wallJumpUpForce) + (targetWallNormal * wallJumpSideForce);

        // Reset y velocity prior to adding force
        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0.0f, _rigidbody.velocity.z);
        // Apply the jump force to the rigidbody
        _rigidbody.AddForce(forceToApply, ForceMode.Impulse);
    }

    private bool AboveGroundThreshold()
    {
        // Return true if the player is above the minimum jump height value, return false if otherwise
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, groundMask);
    }
}
