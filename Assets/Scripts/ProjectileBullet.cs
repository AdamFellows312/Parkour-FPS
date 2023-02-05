using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBullet : MonoBehaviour
{
    private bool terminated;

    private Color bulletColour;

    private Rigidbody _rigidbody;
    private TrailRenderer _trailRenderer;

    private void Awake()
    {
        // Get the bullet's rigidbody
        _rigidbody = this.GetComponent<Rigidbody>();
        _trailRenderer = this.GetComponent<TrailRenderer>();
    }

    private void Update()
    {
        // Move this bullet forward via adding a force
        _trailRenderer = this.GetComponent<TrailRenderer>();
    }

    public void SetBullet(Color colour)
    {
        bulletColour = colour;

        // Return if this bullet doesn't have a trail renderer
        if (_trailRenderer = null)
        {
            Debug.LogWarning("This bullet does not have a trail renderer");
            return;
        }
        // Change the colour of this bullet's trail
        //_trailRenderer.startColor = colour;
        //_trailRenderer.endColor = colour;
    }

    private void OnCollisionEnter(Collision other)
    {
        // If this bullet has already terminated, return
        if (terminated) { return; }

        // Denote the bullet as terminated
        terminated = true;

        int otherLayerIndex = other.gameObject.layer;
        // Get if the bullet has hit the player or another object
        if (otherLayerIndex == LayerMask.NameToLayer("Player"))
        {
            //@TODO: Kill the player
            // Destroy the bullet object
            KillBullet(this.gameObject);
            // Exit the function as the bullet has hit someting
            return;
        }
        // Else, has hit anything other than an enemy
        else if (otherLayerIndex != LayerMask.NameToLayer("Enemy"))
        {
            // If has hit another bullet
            if (otherLayerIndex == LayerMask.NameToLayer("ProjectileBullet"))
            {
                // Destroy this bullet
                KillBullet(this.gameObject);
                // Destroy the other bullet
                KillBullet(other.gameObject);
                // Return as the bullet has hit something
                return;
            }
            KillBullet(this.gameObject);
            return;
        }
        // Hitting an enemy is now assumed as the previous conditionals check if anything other than an enemy was hit
        //@TODO: Enemy hit code
        // Destroy the bullet
        KillBullet(this.gameObject);
    }

    private void KillBullet(GameObject bullet) => Destroy(bullet);
}
