using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrappling : MonoBehaviour
{
    private SpringJoint joint;

    private Vector3 swingPoint;
    private Vector3 currentGrapplePosition;

    private List<Transform> grapplePoints = new List<Transform>();

    private Transform closestGrapplePoint;

    [SerializeField] private float grappleRange = 25.0f;
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

        // Get all the grapple points in the scene
        foreach (var point in GameObject.FindGameObjectsWithTag("GrapplePoint"))
        {
            grapplePoints.Add(point.transform);
        }
    }

    // Called every frame the application is running
    private void Update()
    {
        GetInput();
        UpdateNearestGrapplePoint();
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

    private void UpdateNearestGrapplePoint()
    {
        // Get the closest grapple point
        float minDistance = grappleRange;
        Transform minPoint = null;

        foreach (Transform point in grapplePoints)
        {
            float distance = Vector3.Distance(transform.position, point.position);
            if (distance < minDistance && point.GetComponent<ObjectVisibilityQuery>().IsVisbile())
            {
                minPoint = point;
                //minDistance = distance;
            }
            closestGrapplePoint = null;
        }
        if (minPoint)
        {
            // Only update the closest grapple point if it is on screen
            if (minPoint.GetComponent<ObjectVisibilityQuery>().IsVisbile())
            { 
                closestGrapplePoint = minPoint;
            }
            else
            {
                // Don't update the object if it is not on screen
                closestGrapplePoint = null;
            }
        }
    }

    private void StartSwing()
    {
        // Don't run the function if there is no close grapple point
        if (closestGrapplePoint == null) { return; }

        PlayerMovement.instance.isSwinging = true;

        // Get the swing point 
        swingPoint = closestGrapplePoint.position;
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
