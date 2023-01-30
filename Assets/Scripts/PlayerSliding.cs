using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSliding : MonoBehaviour
{
    private float horizontalInput;
    private float verticalInput;
    private float slideTimer;
    private float yScaleOriginal;

    private Rigidbody rigidbody;
    private PlayerMovement movement;

    [SerializeField] private float maxSlideTime = 0.75f;
    [SerializeField] private float slideForce = 200.0f;
    [SerializeField] private float yScaleSlide = 0.5f;
    [Space(10)]

    [SerializeField] private Transform orientation;
    //[SerializeField] private Transform playerTransform;

    private void Start()
    {
        // Get the player's Rigidbody
        rigidbody = this.GetComponent<Rigidbody>();
        movement = this.GetComponent<PlayerMovement>();

        // Store the initial Y scale of the player
        yScaleOriginal = transform.localScale.y;
    }

    // Called every frame the application is running
    private void Update()
    {
        GetInput();
    }


    // Called every fixed framerate update 
    private void FixedUpdate()
    {
        if (movement.isSliding) { SlidingMovement(); }
    }

    private void GetInput()
    {
        // Get WASD and arrow keys input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Start sliding
        if (Input.GetKeyDown(KeyCode.LeftShift) && (horizontalInput != 0.0f || verticalInput != 0.0f))
        {
            StartSlide();
        }

        // End Sliding
        if (Input.GetKeyUp(KeyCode.LeftShift) && movement.isSliding)
        {
            EndSlide();
        }
    }

    private void StartSlide()
    {
        movement.isSliding = true;

        // Shrink the player on y-axis to the slide scale
        transform.localScale = new Vector3(transform.localScale.x, yScaleSlide, transform.localScale.z);
        // Add downwards force so the player isn't floating right after crouching
        rigidbody.AddForce(Vector3.down * 5.0f, ForceMode.Impulse);

        // Reset the slide timer
        slideTimer = maxSlideTime;
    }

    private void EndSlide()
    {
        // No longer sliding
        movement.isSliding = false;
        // Reset the y scale of the player
        transform.localScale = new Vector3(transform.localScale.x, yScaleOriginal, transform.localScale.z);
    }

    private void SlidingMovement()
    {
        // Get the input direction based on the orientations rotation
        Vector3 inputDirection = (orientation.forward * verticalInput) + (orientation.right * horizontalInput);
        inputDirection.Normalize();

        // Normal sliding when not on a slope (or moving upwards)
        if (!movement.OnSlope() || rigidbody.velocity.y > -0.1f)
        {
            // Apply the slide force
            rigidbody.AddForce(inputDirection * slideForce, ForceMode.Force);
            // Decrease the slide timer
            slideTimer -= Time.deltaTime;
        }
        else
        {
            rigidbody.AddForce(movement.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
            // Not counting down the timer to allow a slide on a slope to last for an undefined duration
        }

        // When the timer runs out, end the slide
        if (slideTimer <= 0.0f) { EndSlide(); }
    }
}
