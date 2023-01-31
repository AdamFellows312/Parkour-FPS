using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For an object to billboard it essentially just has to look at the target object
public class ObjectBillboard : MonoBehaviour
{
    private Transform targetTransform;

    private void Start()
    {
        // Get the target object
        targetTransform = PlayerCamera.instance.transform;
    }

    private void Update()
    {
        Billboard();
    }

    private void Billboard()
    {
        // Look towards the target transform position
        transform.LookAt(targetTransform.position);
    }
}
