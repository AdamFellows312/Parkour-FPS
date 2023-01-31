using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectVisibilityQuery : MonoBehaviour
{
    private Renderer _renderer;

    [SerializeField] private bool onScreen;

    private void Start()
    {
        _renderer = this.GetComponent<Renderer>();
    }

    private void Update()
    {
        // Check visibility of object by checking if it is on screen (within defined bounds)
        Vector3 screenPosition = PlayerCamera.instance.GetComponent<Camera>().WorldToScreenPoint(transform.position);
        onScreen = screenPosition.x > 0f && screenPosition.x < Screen.width && screenPosition.y > 0f && screenPosition.y < Screen.height;

        Debug.Log(name + " is visible: " + IsVisbile());
    }

    public bool IsVisbile()
    {
        // Return visibility
        if (onScreen && _renderer.isVisible)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
