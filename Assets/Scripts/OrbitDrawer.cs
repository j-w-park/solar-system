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
    [Min(0)] public int numSteps = 100;
    public CelestialBody relativeTo = null; // if set, draw orbits relative to this body

    private int relativeIndex = -1;
    private CelestialBody[] bodies;
    private Vector3[][] trails;

    // snapshots
    private Vector3[] pos;
    private Vector3[] vel;
    private Vector3[] acc;
    private float[] mass;
    private float[] radius;

    private bool hasCollision;
    private int colA = -1, colB = -1, colStep = -1;
    private int stepsUsed = 0;

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
        this.bodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.None);
        if (this.bodies == null || this.bodies.Length == 0)
        {
            return;
        }

        this.relativeIndex = -1;
        if (this.relativeTo != null)
        {
            for (int i = 0; i < this.bodies.Length; ++i)
            {
                if (this.bodies[i] == this.relativeTo)
                {
                    this.relativeIndex = i;
                    break;
                }
            }
        }

        var n = this.bodies.Length;

        this.pos = new Vector3[n];
        this.vel = new Vector3[n];
        this.acc = new Vector3[n];
        this.mass = new float[n];
        this.radius = new float[n];

        for (int i = 0; i < n; ++i)
        {
            this.pos[i] = this.bodies[i].transform.position;
            this.vel[i] = this.bodies[i].initialVelocity;
            this.mass[i] = this.bodies[i].Mass;
            this.radius[i] = this.bodies[i].Radius;
        }

        if (this.trails == null || this.trails.Length != n)
        {
            this.trails = new Vector3[n][];
        }

        // reset collision info
        this.hasCollision = false;
        this.colA = this.colB = -1;
        this.colStep = -1;
        this.stepsUsed = Math.Min(1, this.numSteps); // at least the initial point
    }

    private bool CheckCollision(out int a, out int b)
    {
        a = b = -1;

        int n = this.bodies.Length;
        for (int i = 0; i < n - 1; ++i)
        {
            for (int j = i + 1; j < n; ++j)
            {
                float dist = Vector3.Distance(this.pos[i], this.pos[j]);
                if (dist < this.radius[i] + this.radius[j])
                {
                    a = i;
                    b = j;
                    return true;
                }
            }
        }

        return false;
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
            this.trails[i][0] = this.pos[i];
        }

        // leapfrog: initial half-kick
        NBodyLeapfrog.ComputeAccelerations(pos, mass, acc);
        NBodyLeapfrog.InitialHalfKick(vel, acc, Time.fixedDeltaTime);

        // main steps
        for (int @is = 1; @is < this.numSteps; ++@is)
        {
            // drift
            NBodyLeapfrog.Drift(pos, vel, Time.fixedDeltaTime);

            // record position
            for (int i = 0; i < n; ++i)
            {
                this.trails[i][@is] = this.pos[i];
            }

            this.stepsUsed = @is + 1;

            // collision check
            if (this.CheckCollision(out this.colA, out this.colB))
            {
                this.hasCollision = true;
                this.colStep = @is;
                break;
            }

            NBodyLeapfrog.ComputeAccelerations(pos, mass, acc);

            // full kick except last
            if (@is < numSteps - 1)
            {
                NBodyLeapfrog.FullKick(vel, acc, Time.fixedDeltaTime);
            }
        }

        if (!this.hasCollision)
        {
            // final half-kick to keep positions/velocities time-centered
            NBodyLeapfrog.InitialHalfKick(vel, acc, Time.fixedDeltaTime);
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
            // draw only up to stepsUsed
            for (int @is = 1; @is < this.stepsUsed; ++@is)
            {
                Vector3 p0 = this.trails[i][@is - 1];
                Vector3 p1 = this.trails[i][@is];
                if (this.relativeIndex >= 0)
                {
                    p0 = p0 - this.trails[this.relativeIndex][@is - 1] + anchor;
                    p1 = p1 - this.trails[this.relativeIndex][@is] + anchor;
                }
                Gizmos.DrawLine(p0, p1);
            }
        }

        if (this.hasCollision && this.colA >= 0 && this.colB >= 0 && this.colStep >= 0)
        {
            Gizmos.color = Color.red;
            Vector3 pa = this.trails[this.colA][this.colStep];
            Vector3 pb = this.trails[this.colB][this.colStep];
            if (this.relativeIndex >= 0)
            {
                var pref = this.trails[this.relativeIndex][this.colStep];
                pa = pa - pref + anchor;
                pb = pb - pref + anchor;
            }
            float rA = this.radius[this.colA];
            float rB = this.radius[this.colB];
            Gizmos.DrawSphere(pa, rA);
            Gizmos.DrawSphere(pb, rB);
        }
    }
}
