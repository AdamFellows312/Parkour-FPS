using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance;

    [HideInInspector] public bool canJump;
    private bool exitingSlope;
    private bool isCrouching; // Begrudginly, because enums can't be used as conditionals in ternary operators :(
    [HideInInspector] public bool isSliding;
    [HideInInspector] public bool isWallrunnning;
    [HideInInspector] public bool isDashing;
    [HideInInspector] public bool isSwinging;

    private float horizontalInput;
    private float verticalInput;
    private float yScaleOriginal;

    private float movementSpeed;
    private float desiredMovementSpeed;
    private float lastDesiredMoveSpeed;

    private Rigidbody _rigidbody;

    private RaycastHit slopeHit;

    private Vector3 movementDirection;

    public float baseSpeed = 7.0f;
    public float crouchSpeed = 3.5f;
    public float slideSpeed = 30.0f;
    public float wallRunSpeed = 8.5f;
    public float dashSpeed = 20.0f;
    public float swingSpeed;
    [Space(10)]

    [SerializeField] private float playerHeight = 2.0f;
    [SerializeField] private float groundDrag = 4.0f;
    [SerializeField] private float speedIncreaseMultiplier = 1.5f;
    [SerializeField] private float slopeIncreaseMultiplier = 2.5f;
    [SerializeField] private float dashIncreaseMultiplier = 50.0f;

    [SerializeField] private float jumpForce = 18.0f;
    [SerializeField] private float jumpCooldown = 0.25f;
    [SerializeField] private float airMultiplier = 0.4f;

    [SerializeField] private float yScaleCrouch = 0.5f;

    [SerializeField] private float maxSlopeAngle = 45.0f;
    [Space(10)]

    [SerializeField] private Transform orientation;
    [Space(10)]

   public LayerMask groundMask;
    [Space(10)]

    public MovementState state;
    public enum MovementState { grounded, swinging, airborne, crouching, sliding, wallrunning, dashing }

    // Called before Start() 
    private void Awake()
    {
        if (instance == null) { instance = this; }
    }

    // Called on the first frame before Update()
    private void Start()
    {
        // Get the player's Rigidbody
        _rigidbody = this.GetComponent<Rigidbody>();
        _rigidbody.freezeRotation = true;

        // Store the initial Y scale of the player
        yScaleOriginal = transform.localScale.y;

        // Allow the player to jump
        canJump = true;
    }

    // Called every frame the application is running
    private void Update()
    {
        GetInput();
        StateHandler();
        LimitVelocity();

        // Update drag based on whether or not the player is grounded
        if (state == MovementState.grounded || state == MovementState.crouching || state == MovementState.sliding) { _rigidbody.drag = groundDrag; }
        else { _rigidbody.drag = 0.0f; }
    }

    // Called every fixed framerate update 
    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void GetInput()
    {
        // Get WASD and arrow keys input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Allow for jumping
        if (Input.GetKey(KeyCode.Space) && IsGrounded() && canJump) 
        {
            Jump();
            // Reset the jump after a given amount of time has elapsed
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Start crouching
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            // Update the y scale of the player to be crouching
            transform.localScale = new Vector3(transform.localScale.x, yScaleCrouch, transform.localScale.z);
            // Add downwards force so the player isn't floating right after crouching
            _rigidbody.AddForce(Vector3.down * 5.0f, ForceMode.Impulse);
        }
        // End crouching
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            // Reset the y scale of the player
            transform.localScale = new Vector3(transform.localScale.x, yScaleOriginal, transform.localScale.z);
            // Reset is crouching bool
            isCrouching = false;
        }
    }

    private void StateHandler()
    {
        // Update the state if is swinging
        if (isSwinging)
        {
            state = MovementState.swinging;
        }
        // Update the state if is dashing
        if (isDashing)
        {
            state = MovementState.dashing;
        }
        // Update the state if is wallrunning
        else if (isWallrunnning)
        {
            state = MovementState.wallrunning;
        }
        // Update the state if is sliding
        else if (isSliding)
        {
            state = MovementState.sliding;

            // Update the desired move speed based on whether or not the player is sliding and on a slope
            if (OnSlope() && _rigidbody.velocity.y < 0.1f) { desiredMovementSpeed = slideSpeed; }
            else { desiredMovementSpeed = baseSpeed; }
        }

        // Determine if crouching, if so appropriately update the state
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            isCrouching = true;
            state = MovementState.crouching;
        }

        // Determine the state of the player based on if they're grounded or not
        else if (IsGrounded())
        {
            state = MovementState.grounded;
        }
        else if (!IsGrounded())
        {
            state = MovementState.airborne;
        }

        // Check if the desired move speed has changed drastically
        if (Mathf.Abs(desiredMovementSpeed - lastDesiredMoveSpeed) > 4.0f)
        {
            StopAllCoroutines();
            // Start the coroutine, simulate the momentum
            StartCoroutine(SmoothlyLerpMovementSpeed());
        }
        // Difference is too negligible to warrant simulating the momentum;
        else 
        { 
            movementSpeed = desiredMovementSpeed; 
        }
        // Update the last desired move speed;
        lastDesiredMoveSpeed = desiredMovementSpeed;
    }

    private void MovePlayer()
    {
        if (state == MovementState.dashing) { return; }
        if (state == MovementState.swinging) { return; }

        // Calcuate movement direction vector, allowing movement to always be relative to the rotation of the orientation object
        movementDirection = (orientation.forward * verticalInput) + (orientation.right * horizontalInput);
        movementDirection.Normalize();

        // Determine the correct movement speed
        desiredMovementSpeed = isSwinging ? swingSpeed : isDashing ? dashSpeed : isWallrunnning ? wallRunSpeed : isCrouching ? crouchSpeed : baseSpeed;

        if (OnSlope() && !exitingSlope)
        {
            // Apply a force in the direction (relative to the slope's incline) of the slope
            _rigidbody.AddForce(GetSlopeMoveDirection(movementDirection) * movementSpeed * 20.0f, ForceMode.Force);

            // Compensate for removal of gravity while on a slope by adding a constant downwards force while moving up said slope
            if (_rigidbody.velocity.y > 0.0f)
            {
                _rigidbody.AddForce(Vector3.down * 80.0f, ForceMode.Force);
            }
        }
        
        if (IsGrounded())
        {
            // Apply a force to move the player
            _rigidbody.AddForce(movementDirection * movementSpeed * 10.0f, ForceMode.Force);
        }
        else if (!IsGrounded())
        {
            _rigidbody.AddForce(movementDirection * movementSpeed * 10.0f * airMultiplier, ForceMode.Force);
        }

        // Remove gravity when on a slope (so the player doesn't slide down the slope) and when not wallrunning
        if (!isWallrunnning) { _rigidbody.useGravity = !OnSlope(); }
    }

    private void LimitVelocity()
    {
        // Limit velocity on the slope
        if (OnSlope() && !exitingSlope)
        {
            if (_rigidbody.velocity.magnitude > movementSpeed)
            {
                _rigidbody.velocity = _rigidbody.velocity.normalized * movementSpeed;
            }
        }
        // Limiting the velocity in the air/on the ground
        else if (!OnSlope())
        {
            // Get the velocity as it is on a plane (no vertical velocity)
            Vector3 flatVelocity = new Vector3(_rigidbody.velocity.x, 0.0f, _rigidbody.velocity.z);

            // Limit the velocity if going over the max speed
            if (flatVelocity.magnitude > movementSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * movementSpeed;
                // Apply the limited velocity to the player's rigidbody
                _rigidbody.velocity = new Vector3(limitedVelocity.x, _rigidbody.velocity.y, limitedVelocity.z);
            }
        }
    }

    private IEnumerator SmoothlyLerpMovementSpeed()
    {
        float time = 0.0f;
        float difference = Mathf.Abs(desiredMovementSpeed - movementSpeed);
        float startValue = movementSpeed;

        while (time < difference)
        {
            // Lerp the movement speed towards the desired movement speed to simulate momentum
            movementSpeed = Mathf.Lerp(startValue, desiredMovementSpeed, time / difference);

            if (OnSlope())
            {
                // Get the angle of the slope
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1.0f + (slopeAngle / 90.0f);
                
                // Increase time, multiply it based on the angle increase and slope increase to increase acceleration
                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else if (!OnSlope() && isDashing)
            {
                // Increase the time multilpied by the dash speed multilpier
                time += Time.deltaTime * dashIncreaseMultiplier;
            }
            else if (!OnSlope() && !isDashing)
            {
                // Increase the time 
                time += Time.deltaTime * speedIncreaseMultiplier;
            }
            yield return null;
        }
        // Directly set the move speed after the lerp to ensure the correct value is met (not a value of x.999...)
        movementSpeed = desiredMovementSpeed;
    }

    private void Jump()
    {
        exitingSlope = true;
        // Don't allow for jumping during a jump
        canJump = false;

        // Reset the y velocity
        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0.0f, _rigidbody.velocity.z);
        // Apply the jumping force
        _rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        canJump = true;
        exitingSlope = false;
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundMask);
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            // Get the angle of the slope
            float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);

            return slopeAngle < maxSlopeAngle && slopeAngle != 0.0f;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        // Retun the moveDirection vetor projected onto the slopes incline surface
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public Rigidbody GetRigidbody()
    {
        return _rigidbody;
    }
}
