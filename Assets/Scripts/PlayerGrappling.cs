using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrappling : MonoBehaviour
{
    private float maxSwingDistance = 25.0f;

    private SpringJoint joint;

    private Vector3 swingPoint;
    private Vector3 currentGrapplePosition;

    [SerializeField] private int quality;
    [SerializeField] private float strength;
    [SerializeField] private float waveCount;
    [SerializeField] private float waveHeight;
    [SerializeField] private float velocity;
    [Space(10)]

    [SerializeField] private AnimationCurve affectCurve;
    [Space(10)]

    [SerializeField] private LineRenderer line;
    [Space(10)]

    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform startPoint;


    // Called on the first frame before Update()
    private void Start()
    {
        playerCamera = PlayerCamera.instance.transform;
        startPoint = PlayerCamera.instance.transform.GetChild(0);
        playerTransform = this.transform;
    }

    // Called every frame the application is running
    private void Update()
    {
        GetInput();
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void GetInput() 
    {
        if (Input.GetMouseButtonDown(1)) { StartSwing(); }
        if (Input.GetMouseButtonUp(1)) { StopSwing(); }
    }

    private void StartSwing()
    {
        PlayerMovement.instance.isSwinging = true;

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, maxSwingDistance))
        {
            // Get the swing point 
            swingPoint = hit.point;
            // Create a spring joint on the player 
            joint = playerTransform.gameObject.AddComponent<SpringJoint>();
            // Configure the spring joint
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = swingPoint;

            // Get the distance from the player to the swing point
            float distanceFromPoint = Vector3.Distance(playerTransform.position, swingPoint);

            // The distance the grapple will try to keep from the grapple point
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 4.5f;
            joint.damper = 7.5f;
            joint.massScale = 4.5f;

            line.positionCount = 2;
            currentGrapplePosition = startPoint.position;
        }
    }

    private void StopSwing()
    {
        PlayerMovement.instance.isSwinging = false;

        line.positionCount = 0;
        Destroy(joint);
    }

    private void DrawRope()
    {
        // If not grappling, don't draw the tope
        if (!joint) { return; }

        line.SetPosition(0, startPoint.position);
        line.SetPosition(1, swingPoint);
    }
}
