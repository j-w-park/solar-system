using UnityEngine;

public static class NBodyLeapfrog
{
    public const float SOFTENING = 0.02f; // avoids singluarity at very small r

    // Arrays must be length n. Mass is "GM" (here surfaceG*radius^2).
    public static void ComputeAccelerations(Vector3[] pos, float[] mass, Vector3[] acc)
    {
        int n = pos.Length;
        for (int i = 0; i < n; ++i)
        {
            acc[i] = Vector3.zero;
        }

        for (int i = 0; i < n - 1; ++i)
        {
            for (int j = i + 1; j < n; ++j)
            {
                Vector3 r = pos[j] - pos[i];
                float r2 = r.sqrMagnitude + SOFTENING * SOFTENING;
                float invR = 1.0f / Mathf.Sqrt(r2);
                float invR3 = invR * invR * invR;

                Vector3 baseVec = r * invR3; // G=1 convention
                acc[i] += baseVec * mass[j];
                acc[j] -= baseVec * mass[i];
            }
        }
    }

    // Do exactly one KDK step:
    //  - If firstStep: perform initial half-kick internally (requires acc already at current pos).
    //  - Otherwise: assumes vel already at half-step (usual loop).
    public static void InitialHalfKick(Vector3[] vel, Vector3[] acc, float dt)
    {
        for (int i = 0; i < vel.Length; ++i)
        {
            vel[i] += 0.5f * dt * acc[i];
        }
    }

    public static void Drift(Vector3[] pos, Vector3[] vel, float dt)
    {
        for (int i = 0; i < pos.Length; ++i)
        {
            pos[i] += dt * vel[i];
        }
    }

    public static void FullKick(Vector3[] vel, Vector3[] acc, float dt)
    {
        for (int i = 0; i < vel.Length; ++i)
        {
            vel[i] += dt * acc[i];
        }
    }
}
