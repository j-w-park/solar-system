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
    [Min(0f)] public float softening = 0.02f; // avoids singluarity at very small r
    public CelestialBody relativeTo = null; // if set, draw orbits relative to this body

    private int relativeIndex = -1;
    private CelestialBody[] bodies;
    private VirtualBody[] vbs; // snapshots
    private Vector3[][] trails;

    private bool hasCollision;
    private int colA = -1, colB = -1, colStep = -1;
    private int stepsUsed = 0;

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
        if (this.vbs == null || this.vbs.Length != n)
        {
            this.vbs = new VirtualBody[n];
        }

        if (this.trails == null || this.trails.Length != n)
        {
            this.trails = new Vector3[n][];
        }

        for (int i = 0; i < n; ++i)
        {
            this.vbs[i] = new VirtualBody
            {
                mass = this.bodies[i].Mass,
                radius = this.bodies[i].Radius,
                position = this.bodies[i].transform.position,
                velocity = this.bodies[i].initialVelocity,
                acceleration = Vector3.zero,
            };
        }

        // reset collision info
        this.hasCollision = false;
        this.colA = this.colB = -1;
        this.colStep = -1;
        this.stepsUsed = Math.Min(1, this.numSteps); // at least the initial point
    }

    private bool CheckCollision(out int a, out int b)
    {
        int n = this.vbs.Length;

        a = b = -1;
        for (int i = 0; i < n - 1; ++i)
        {
            for (int j = i + 1; j < n; ++j)
            {
                float dist = Vector3.Distance(this.vbs[i].position, this.vbs[j].position);
                if (dist < this.vbs[i].radius + this.vbs[j].radius)
                {
                    a = i;
                    b = j;
                    return true;
                }
            }
        }

        return false;
    }

    private void ComputeAccelerations()
    {
        int n = this.vbs.Length;

        for (int i = 0; i < n; ++i)
        {
            this.vbs[i].acceleration = Vector3.zero;
        }

        // pairwise accumulation
        for (int i = 0; i < n - 1; ++i)
        {
            for (int j = i + 1; j < n; ++j)
            {
                Vector3 r = this.vbs[j].position - this.vbs[i].position;
                float r2 = r.sqrMagnitude + this.softening * this.softening;
                float invR = 1.0f / Mathf.Sqrt(r2);
                float invR3 = invR * invR * invR;

                Vector3 a = r * invR3;

                this.vbs[i].acceleration += a * this.vbs[j].mass;
                this.vbs[j].acceleration -= a * this.vbs[i].mass;
            }
        }
    }

    private void Simulate()
    {
        if (this.vbs == null)
        {
            return;
        }

        int n = this.vbs.Length;
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
            this.trails[i][0] = this.vbs[i].position;
        }

        // leapfrog: initial half-kick
        this.ComputeAccelerations();
        for (int i = 0; i < n; ++i)
        {
            this.vbs[i].velocity += this.vbs[i].acceleration * (Time.fixedDeltaTime * 0.5f);
        }

        // main steps
        for (int @is = 1; @is < this.numSteps; ++@is)
        {
            // drift
            for (int i = 0; i < n; ++i)
            {
                this.vbs[i].position += this.vbs[i].velocity * Time.fixedDeltaTime;
            }

            // record position
            for (int i = 0; i < n; ++i)
            {
                this.trails[i][@is] = this.vbs[i].position;
            }

            this.stepsUsed = @is + 1;

            // collision check
            if (this.CheckCollision(out this.colA, out this.colB))
            {
                this.hasCollision = true;
                this.colStep = @is;
                break;
            }

            this.ComputeAccelerations();

            // full kick except last
            if (@is < numSteps - 1)
            {
                for (int i = 0; i < n; ++i)
                {
                    this.vbs[i].velocity += this.vbs[i].acceleration * Time.fixedDeltaTime;
                }
            }
        }

        if (!this.hasCollision)
        {
            // final half-kick to keep positions/velocities time-centered
            for (int i = 0; i < n; ++i)
            {
                this.vbs[i].velocity += this.vbs[i].acceleration * (Time.fixedDeltaTime * 0.5f);
            }
        }
    }

    private void DrawTrails()
    {
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
            float rA = this.vbs[this.colA].radius;
            float rB = this.vbs[this.colB].radius;
            Gizmos.DrawSphere(pa, rA);
            Gizmos.DrawSphere(pb, rB);
        }
    }


    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            if (this.trails == null)
            {
                Init();
                Simulate();
            }
        }
        else
        {
            Init();
            Simulate();
        }

        DrawTrails();
    }
}
