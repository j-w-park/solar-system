using UnityEngine;

public class SolarSystem : MonoBehaviour
{
    private CelestialBody[] bodies;

    private void Awake()
    {
        this.bodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.None);
    }

    private void FixedUpdate()
    {
        foreach (var b in this.bodies)
        {
            b.UpdateVelocity(ref this.bodies);
        }
        foreach (var b in this.bodies)
        {
            b.UpdatePosition();
        }
    }
}
