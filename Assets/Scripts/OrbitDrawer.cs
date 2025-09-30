using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class OrbitDrawer : MonoBehaviour
{
    [Min(0)] public int numSegments = 100;

    private CelestialBody[] allBodies;

    private List<Trail> trails;

    class VirtualBody
    {
        public float mass;
        public float radius;
        public Vector3 position;
        public Vector3 velocity;
    }

    class TrailPoint
    {
        public Vector3 position;
        public bool isCollided = false;
    }

    class Trail
    {
        public VirtualBody target;
        public TrailPoint[] points;

        public Trail(VirtualBody target, int numPoints)
        {
            this.target = target;
            this.points = new TrailPoint[numPoints];
        }
    }

    private void CalculateTrails()
    {
        this.trails = new List<Trail>(this.allBodies.Length);
        for (int i = 0; i < this.allBodies.Length; ++i)
        {
            var target = new VirtualBody
            {
                mass = this.allBodies[i].mass,
                radius = this.allBodies[i].transform.localScale.x * 0.5f,
                position = this.allBodies[i].transform.position,
                velocity = this.allBodies[i].initialVelocity
            };
            this.trails.Add(new Trail(target, this.numSegments));
        }

        for (int i = 0; i < this.numSegments; ++i)
        {
            // update velocity
            for (int j = 0; j < this.allBodies.Length; ++j)
            {
                var current = this.trails[j];
                for (int k = 0; k < this.allBodies.Length; ++k)
                {
                    var other = this.trails[k];
                    if (current.target != other.target)
                    {
                        Vector3 disp = other.target.position - current.target.position;
                        Vector3 acc = disp.normalized * this.allBodies[k].mass / disp.sqrMagnitude;
                        current.target.velocity += acc * Time.fixedDeltaTime;

                    }
                }
            }

            // update position
            for (int j = 0; j < this.allBodies.Length; ++j)
            {
                Trail t = this.trails[j];
                t.target.position += t.target.velocity * Time.fixedDeltaTime;
                t.points[i] = new TrailPoint
                {
                    position = t.target.position,
                    isCollided = false,
                };
            }

            // check for collision
            for (int j = 0; j < this.allBodies.Length; ++j)
            {
                Trail current = this.trails[j];
                for (int k = 0; k < this.allBodies.Length; ++k)
                {
                    Trail other = this.trails[k];
                    if (current.target == other.target)
                    {
                        continue;
                    }

                    float dist = Vector3.Distance(current.target.position, other.target.position);
                    if (dist < current.target.radius + other.target.radius)
                    {
                        current.points[i].isCollided = true;
                    }
                }
            }

        }
    }

    private void OnDrawGizmos()
    {
        this.allBodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.None);
        Debug.Log(this.allBodies.Length);

        if (!Application.isPlaying)
        {
            this.CalculateTrails();
        } else if (this.trails == null || this.trails.Count != this.allBodies.Length)
        {
            this.CalculateTrails();
        }

        Debug.Log(this.trails);

        for (int i = 0; i < this.allBodies.Length; ++i)
        {
            Gizmos.color = Color.HSVToRGB((float)i / this.allBodies.Length, 1.0f, 1.0f);
            for (int j = 1; j < this.trails[i].points.Length; ++j)
            {
                Gizmos.DrawLine(this.trails[i].points[j - 1].position, this.trails[i].points[j].position);
            }

            Gizmos.color = Color.red;
            for (int j = 0; j < this.trails[i].points.Length; ++j)
            {
                if (this.trails[i].points[j].isCollided)
                {
                    Gizmos.DrawSphere(this.trails[i].points[j].position, this.trails[i].target.radius * 0.5f);
                }
            }
        }
    }
}
