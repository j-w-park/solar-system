using System;
using UnityEngine;

public struct VirtualBody
{
    public float mass;
    public float radius;
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration;
}

public class OrbitDrawer : MonoBehaviour
{
    [Min(0)] public int numSteps = 10_000;
    [ReadOnly] public int step = 0;
    [Range(0.02f, 1f)] public float deltaTime = 0.02f;
    public CelestialBody relativeTo = null; // if set, draw orbits relative to this body

    private CelestialBody[] bodies;

    private NBodyLeapfrog leapfrogSolver;

    private int relativeIndex = -1;
    private Vector3[][] trails;

    private int colA = -1, colB = -1;

    private void Awake()
    {
        if (Application.isPlaying)
        {
            this.Init();
            this.Simulate();
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            this.Init();
            this.Simulate();
        }
        this.DrawTrails();
    }

    private void Init()
    {
        this.bodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.InstanceID);
        if (this.bodies == null || this.bodies.Length == 0)
        {
            return;
        }

        this.leapfrogSolver = new NBodyLeapfrog(this.bodies, this.deltaTime);

        int n = this.bodies.Length;

        if (this.trails == null || this.trails.Length != n)
        {
            this.trails = new Vector3[n][];
        }

        this.relativeIndex = -1;
        if (this.relativeTo != null)
        {
            for (int i = 0; i < n; ++i)
            {
                if (this.bodies[i] == this.relativeTo)
                {
                    this.relativeIndex = i;
                    break;
                }
            }
        }
    }

    private void Simulate()
    {
        int n = this.bodies.Length;
        if (n == 0 || this.numSteps < 2)
        {
            return;
        }

        for (int i = 0; i < n; ++i)
        {
            if (this.trails[i] == null || this.trails[i].Length != this.numSteps)
            {
                this.trails[i] = new Vector3[this.numSteps];
            }
            this.trails[i][0] = this.leapfrogSolver.Pos[i];
        }

        // leapfrog: initial half-kick
        this.leapfrogSolver.ComputeAccelerations();
        this.leapfrogSolver.InitialHalfKick();

        // main steps
        for (this.step = 1; this.step < this.numSteps; ++this.step)
        {
            // drift
            this.leapfrogSolver.Drift();

            // record position
            for (int i = 0; i < n; ++i)
            {
                this.trails[i][this.step] = this.leapfrogSolver.Pos[i];
            }

            // collision check
            this.colA = this.colB = -1;
            if (this.leapfrogSolver.CheckCollision(out this.colA, out this.colB))
            {
                break;
            }

            this.leapfrogSolver.ComputeAccelerations();

            // full kick except last
            if (this.step < numSteps - 1)
            {
                this.leapfrogSolver.FullKick();
            }
        }

        if (this.colA <= 0 || this.colB <= 0)
        {
            // final half-kick to keep positions/velocities time-centered
            this.leapfrogSolver.InitialHalfKick();
        }
    }

    private void DrawTrails()
    {
        if (this.trails == null || this.bodies == null)
        {
            return;
        }

        int n = this.trails.Length;

        var anchor = Vector3.zero;
        if (this.relativeIndex >= 0)
        {
            anchor = this.bodies[this.relativeIndex].transform.position;
        }

        for (int i = 0; i < n; ++i)
        {
            Gizmos.color = Color.HSVToRGB((float)i / Mathf.Max(1, n), 1f, 1f);
            // draw only up to steps computed
            for (int s = 1; s < this.step; ++s)
            {
                Vector3 p0 = this.trails[i][s - 1];
                Vector3 p1 = this.trails[i][s];
                if (this.relativeIndex >= 0)
                {
                    p0 = p0 - this.trails[this.relativeIndex][s - 1] + anchor;
                    p1 = p1 - this.trails[this.relativeIndex][s] + anchor;
                }
                Gizmos.DrawLine(p0, p1);
            }
        }

        if (this.colA >= 0 && this.colB >= 0)
        {
            Gizmos.color = Color.red;
            Vector3 pa = this.trails[this.colA][this.step];
            Vector3 pb = this.trails[this.colB][this.step];
            if (this.relativeIndex >= 0)
            {
                Vector3 pref = this.trails[this.relativeIndex][this.step];
                pa = pa - pref + anchor;
                pb = pb - pref + anchor;
            }
            float rA = this.bodies[this.colA].Radius;
            float rB = this.bodies[this.colB].Radius;
            Gizmos.DrawSphere(pa, rA);
            Gizmos.DrawSphere(pb, rB);
        }
    }
}
