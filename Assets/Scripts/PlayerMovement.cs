using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private bool canJump;

    private float horizontalInput;
    private float verticalInput;

    private Rigidbody rigidbody;

    private Vector3 movementDirection;

    public float movementSpeed = 7.0f;

    [SerializeField] private float playerHeight = 2.0f;
    [SerializeField] private float groundDrag = 5.0f;

    [SerializeField] private float jumpForce = 18.0f;
    [SerializeField] private float jumpCooldown = 0.25f;
    [SerializeField] private float airMultiplier = 0.4f;
    [Space(10)]

    [SerializeField] private Transform orientation;
    [Space(10)]

    [SerializeField] private LayerMask groundMask;

    // Called on the first frame before Update()
    private void Start()
    {
        // Get the player's Rigidbody;
        rigidbody = this.GetComponent<Rigidbody>();
        rigidbody.freezeRotation = true;

        // Allow the player to jump
        canJump = true;
    }

    // Called every frame the application is running
    private void Update()
    {
        GetInput();
        LimitVelocity();

        // Update drag based on whether or not the player is grounded
        if (IsGrounded()) { rigidbody.drag = groundDrag; }
        else if (!IsGrounded()) { rigidbody.drag = 0.0f; }
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
    }

    private void MovePlayer()
    {
        // Calcuate movement direction vector, allowing movement to always be relative to the rotation of the orientation object
        movementDirection = (orientation.forward * verticalInput) + (orientation.right * horizontalInput);
        movementDirection.Normalize();

        if (IsGrounded())
        {
            // Apply a force to move the player
            rigidbody.AddForce(movementDirection * movementSpeed * 10.0f, ForceMode.Force);
        }
        else if (!IsGrounded())
        {
            rigidbody.AddForce(movementDirection * movementSpeed * 10.0f * airMultiplier, ForceMode.Force);
        }
    }

    private void Jump()
    {
        // Don't allow for jumping during a jump
        canJump = false;

        // Reset the y velocity
        rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0.0f, rigidbody.velocity.z);
        // Apply the jumping force
        rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump() => canJump = true;

    private void LimitVelocity()
    {
        // Get the velocity as it is on a plane (no vertical velocity)
        Vector3 flatVelocity = new Vector3(rigidbody.velocity.x, 0.0f, rigidbody.velocity.z);

        // Limit the velocity if going over the max speed
        if (flatVelocity.magnitude > movementSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * movementSpeed;
            // Apply the limited velocity to the player's rigidbody
            rigidbody.velocity = new Vector3(limitedVelocity.x, rigidbody.velocity.y, limitedVelocity.z);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundMask);
    }
}
