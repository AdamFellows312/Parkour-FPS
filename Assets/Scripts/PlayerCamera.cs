using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    // Float values that store the camera's x and y rotation
    private float xRotation;
    private float yRotation;

    // Serialized float values that control camera sensitivity 
    [SerializeField] private float sensitivityX = 200.0f;
    [SerializeField] private float sensitivityY = 200.0f;
    [Space(10)]

    [SerializeField] private Transform orientation;
    [SerializeField] private Transform targetCameraPosition;

    // Called on the first frame before Update()
    private void Start()
    {
        // Lock the position of the cursor and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Set the transforms (because prefab saving wont allow instances outside the parent object)
        orientation = GameObject.Find("Orientation").transform;
        targetCameraPosition = GameObject.Find("CameraPosition").transform;
    }

    // Called every frame the application is running
    private void Update()
    {
        MoveCamera();

        // Always go to the target position if the target trasnform is assigned
        if (targetCameraPosition) { transform.parent.position = targetCameraPosition.position; }

        // Allow toggling of cursor visibility (because my escpe key is dead)
        if (Input.GetKeyDown(KeyCode.T))
        {
            switch(Cursor.lockState)
            {
                // Unlock cursor if locked
                case CursorLockMode.Locked:
                    Cursor.lockState = CursorLockMode.None;
                    break;
                // Lock cursor if unlocked
                case CursorLockMode.None:
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
            }
            // Set cursor visibility to inverse of current visibility
            Cursor.visible = !Cursor.visible;
        }
    }

    private void MoveCamera()
    {
        // Get the mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivityX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivityY;

        // Update the rotation of the camera based on the mouse input
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90.0f, 90.0f);

        // Rotate the camera and the orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0.0f);
        orientation.rotation = Quaternion.Euler(0.0f, yRotation, 0.0f);
    }
}
