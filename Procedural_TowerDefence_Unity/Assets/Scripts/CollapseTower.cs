using UnityEngine;
using UnityEngine.InputSystem;

public class CollapseTower : MonoBehaviour
{
    public Rigidbody[] blocks;
    public Transform[] explosionLocations;
    public float explosionForce = 500f;
    public float explosionRadius = 5f;

    void Start()
    {
        // Disable physics at first
        foreach (var rb in blocks)
            rb.isKinematic = true;
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // Enable physics
            foreach (var rb in blocks)
                rb.isKinematic = false;

            // Apply explosion force
            foreach (var rb in blocks)
            {
                foreach (var explosionLocation in explosionLocations)
                {
                    rb.AddExplosionForce(
                        explosionForce,
                        explosionLocation.position,
                        explosionRadius
                    );
                }
            }
        }
    }
}
