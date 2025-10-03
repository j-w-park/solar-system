using UnityEngine;

public class SolarSystem : MonoBehaviour
{
    private CelestialBody[] bodies;
    private bool initialized = false;

    private NBodyLeapfrog leapfrogSolver;

    private void Awake()
    {
        this.bodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.InstanceID);
        if (this.bodies == null || this.bodies.Length == 0)
        {
            return;
        }
        this.leapfrogSolver = new NBodyLeapfrog(this.bodies);
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
            this.leapfrogSolver.ComputeAccelerations();
            this.leapfrogSolver.InitialHalfKick();
            this.initialized = true;
        }

        // drift
        this.leapfrogSolver.Drift();

        // update positions
        for (int i = 0; i < this.bodies.Length; ++i)
        {
            this.bodies[i].transform.position = this.leapfrogSolver.Pos[i];
        }

        // full kick
        this.leapfrogSolver.ComputeAccelerations();
        this.leapfrogSolver.FullKick();
    }
}
