using UnityEngine;

public class SolarSystem : MonoBehaviour
{
    private CelestialBody[] bodies;
    private bool initialized = false;

    // snapshots
    private Vector3[] pos;
    private Vector3[] vel;
    private Vector3[] acc;
    private float[] mass;

    private void Awake()
    {
        this.bodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.None);
        if (this.bodies == null || this.bodies.Length == 0)
        {
            return;
        }

        int n = this.bodies.Length;
        this.pos = new Vector3[n];
        this.vel = new Vector3[n];
        this.acc = new Vector3[n];
        this.mass = new float[n];

        for (int i = 0; i < n; ++i)
        {
            this.pos[i] = this.bodies[i].transform.position;
            this.vel[i] = this.bodies[i].initialVelocity;
            this.mass[i] = this.bodies[i].Mass;
        }
    }

    private void FixedUpdate()
    {
        if (this.bodies == null || this.bodies.Length == 0)
        {
            return;
        }

        // initial half-kick
        if (!this.initialized)
        {
            NBodyLeapfrog.ComputeAccelerations(this.pos, this.mass, this.acc);
            NBodyLeapfrog.InitialHalfKick(this.vel, this.acc, Time.fixedDeltaTime);
            this.initialized = true;
        }

        // drift
        NBodyLeapfrog.Drift(this.pos, this.vel, Time.fixedDeltaTime);
        for (int i = 0; i < this.bodies.Length; ++i)
        {
            this.bodies[i].transform.position = this.pos[i];
        }
        // full kick
        NBodyLeapfrog.ComputeAccelerations(this.pos, this.mass, this.acc);
        NBodyLeapfrog.FullKick(this.vel, this.acc, Time.fixedDeltaTime);
    }
}
