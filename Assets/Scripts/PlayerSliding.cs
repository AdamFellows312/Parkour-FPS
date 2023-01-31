using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSliding : MonoBehaviour
{
    private float horizontalInput;
    private float verticalInput;
    private float slideTimer;
    private float yScaleOriginal;

    private Rigidbody _rigidbody;

    [SerializeField] private float maxSlideTime = 0.75f;
    [SerializeField] private float slideForce = 200.0f;
    [SerializeField] private float yScaleSlide = 0.5f;
    [Space(10)]

    [SerializeField] private Transform orientation;
    //[SerializeField] private Transform playerTransform;

    // Called on the first frame before Update()
    private void Start()
    {
        // Get the player's Rigidbody
        _rigidbody = PlayerMovement.instance.GetRigidbody();

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
        if (PlayerMovement.instance.isSliding) { SlidingMovement(); }
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
        if (Input.GetKeyUp(KeyCode.LeftShift) && PlayerMovement.instance.isSliding)
        {
            EndSlide();
        }
    }

    private void StartSlide()
    {
        PlayerMovement.instance.isSliding = true;

        // Shrink the player on y-axis to the slide scale
        transform.localScale = new Vector3(transform.localScale.x, yScaleSlide, transform.localScale.z);
        // Add downwards force so the player isn't floating right after crouching
        _rigidbody.AddForce(Vector3.down * 5.0f, ForceMode.Impulse);

        // Reset the slide timer
        slideTimer = maxSlideTime;
    }

    private void EndSlide()
    {
        // No longer sliding
        PlayerMovement.instance.isSliding = false;
        // Reset the y scale of the player
        transform.localScale = new Vector3(transform.localScale.x, yScaleOriginal, transform.localScale.z);
    }

    private void SlidingMovement()
    {
        // Get the input direction based on the orientations rotation
        Vector3 inputDirection = (orientation.forward * verticalInput) + (orientation.right * horizontalInput);
        inputDirection.Normalize();

        // Normal sliding when not on a slope (or moving upwards)
        if (!PlayerMovement.instance.OnSlope() || _rigidbody.velocity.y > -0.1f)
        {
            // Apply the slide force
            _rigidbody.AddForce(inputDirection * slideForce, ForceMode.Force);
            // Decrease the slide timer
            slideTimer -= Time.deltaTime;
        }
        else
        {
            _rigidbody.AddForce(PlayerMovement.instance.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
            // Not counting down the timer to allow a slide on a slope to last for an undefined duration
        }

        // When the timer runs out, end the slide
        if (slideTimer <= 0.0f) { EndSlide(); }
    }
}
