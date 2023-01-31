using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GrapplePoint : MonoBehaviour
{
    private GameObject indicator;

    private ObjectVisibilityQuery visibility;

    private Vector3 desiredIndicatorScale = new Vector3(0.015f, 0.015f, 0.015f);

    [SerializeField] private GameObject grappleIndicator;

    private void Start()
    {
        // Get the visibilty indicator 
        visibility = this.GetComponent<ObjectVisibilityQuery>();
    }

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, PlayerMovement.instance.transform.position);
        // Create the grapple point indicator if this grapple point is on screen
        if (visibility.IsVisbile() && indicator == null && distance <= 20.0f)
        {
            // Not using invoke as it will produce multiple instances of the indicator
            CreateIndicator();
        }
        // Destroy it if it exists and if this grapple point is not on screen
        if (!visibility.IsVisbile() && indicator != null)
        {
            Invoke(nameof(DestroyIndicator), 0.2f);
        }

        if (indicator != null)
        {
            // Make the indicator spin
            indicator.transform.GetChild(0).Rotate(0.0f, 0.0f, 150.0f * Time.deltaTime);
        }
    }

    private void CreateIndicator()
    {
        // Instantiate an instance of the indicator
        indicator = Instantiate(grappleIndicator, transform.position, Quaternion.identity, transform);
        
        indicator.transform.GetChild(0).localScale = Vector3.zero;
        indicator.transform.GetChild(0).gameObject.SetActive(true);
        // Have the indicator pop-up when it spawns
        indicator.transform.GetChild(0).DOScale(desiredIndicatorScale, 0.15f);
    }

    private void DestroyIndicator()
    {
        // Return if the indicator is null
        if (indicator == null) { return; }
        // Destroy the instantiated instance of the indicator
        Destroy(indicator.gameObject);
        // Reset the stored indicator to null
        indicator = null;
    }
}
