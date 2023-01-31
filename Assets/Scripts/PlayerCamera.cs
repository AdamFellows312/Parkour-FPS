using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // Using DOTween for easier implementation

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera instance;

    // Float values that store the camera's x and y rotation
    private float xRotation;
    private float yRotation;

    // Serialized float values that control camera sensitivity 
    [SerializeField] private float sensitivityX = 200.0f;
    [SerializeField] private float sensitivityY = 200.0f;
    [Space(10)]

    [SerializeField] private Transform orientation;
    [SerializeField] private Transform targetCameraPosition;

    // Called before Start() 
    private void Awake()
    {
        if (instance == null) { instance = this; }
    }

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
        if (targetCameraPosition) { transform.parent.parent.position = targetCameraPosition.position; }

        // Allow toggling of cursor visibility (because my escpe key is dead)
        if (Input.GetKeyUp(KeyCode.T))
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
        transform.parent.parent.rotation = Quaternion.Euler(xRotation, yRotation, 0.0f);
        orientation.rotation = Quaternion.Euler(0.0f, yRotation, 0.0f);
    }

    public void CameraFovChange(float endValue)
    {
        // Get the camera component on this object
        Camera thisCamera = this.GetComponent<Camera>();
        // Usin DOTween, change the FOV value of the camera to the end value
        thisCamera.DOFieldOfView(endValue, 0.25f);
    }

    public void CameraTilt(float endTilt)
    {
        transform.parent.DOLocalRotate(new Vector3(0.0f, 0.0f, endTilt), 0.25f);
    }
}
