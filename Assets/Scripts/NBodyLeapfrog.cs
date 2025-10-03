using UnityEngine;
using Unity.Mathematics;
using System;

public class NBodyLeapfrog
{
    public const float G = 1.0f; // gravitational constant
    public const float SOFTENING = 0.02f; // avoids singluarity at very small r

    private readonly int numBodies;
    private readonly Vector3[] pos;
    private readonly Vector3[] vel;
    private readonly Vector3[] acc;
    private readonly float[] mass;
    private readonly float[] radius;

    private readonly float dt;

    public Vector3[] Pos => this.pos;
    public Vector3[] Vel => this.vel;

    public NBodyLeapfrog(CelestialBody[] bodies, float dt = -1f)
    {
        if (bodies == null || bodies.Length == 0)
        {
            return;
        }

        if (this.numBodies == bodies.Length)
        {
            return;
        }

        this.numBodies = bodies.Length;
        this.pos = new Vector3[numBodies];
        this.vel = new Vector3[numBodies];
        this.acc = new Vector3[numBodies];
        this.mass = new float[numBodies];
        this.radius = new float[numBodies];
        for (int i = 0; i < numBodies; ++i)
        {
            this.pos[i] = bodies[i].transform.position;
            this.vel[i] = bodies[i].initialVelocity;
            this.mass[i] = bodies[i].Mass;
            this.radius[i] = bodies[i].Radius;
        }

        this.dt = Time.fixedDeltaTime;
        if (dt > 0f)
        {
            this.dt = dt;
        }
    }

    public void ComputeAccelerations()
    {
        int n = this.pos.Length;
        for (int i = 0; i < n; ++i)
        {
            this.acc[i] = Vector3.zero;
        }

        for (int i = 0; i < n - 1; ++i)
        {
            for (int j = i + 1; j < n; ++j)
            {
                Vector3 r = this.pos[j] - this.pos[i];
                float r2 = r.sqrMagnitude + SOFTENING * SOFTENING;
                float invR = math.rsqrt(r2);
                float invR3 = invR * invR * invR;

                Vector3 a = G * invR3 * r;

                this.acc[i] += a * this.mass[j];
                this.acc[j] -= a * this.mass[i];
            }
        }
    }

    public void InitialHalfKick()
    {
        for (int i = 0; i < this.vel.Length; ++i)
        {
            this.vel[i] += 0.5f * this.dt * acc[i];
        }
    }

    public void Drift()
    {
        for (int i = 0; i < this.pos.Length; ++i)
        {
            this.pos[i] += this.dt * this.vel[i];
        }
    }

    public void FullKick()
    {
        for (int i = 0; i < this.vel.Length; ++i)
        {
            this.vel[i] += this.dt * acc[i];
        }
    }

    public void ForEach(Action<int, Vector3> action)
    {
        for (int i = 0; i < this.numBodies; ++i)
        {
            action(i, this.pos[i]);
        }
    }

    public void ForEach(Action<int, Vector3, Vector3> action)
    {
        for (int i = 0; i < this.numBodies; ++i)
        {
            action(i, this.pos[i], this.vel[i]);
        }
    }

    public bool CheckCollision(out int a, out int b)
    {
        a = b = -1;

        for (int i = 0; i < this.numBodies - 1; ++i)
        {
            for (int j = i + 1; j < this.numBodies; ++j)
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
}
