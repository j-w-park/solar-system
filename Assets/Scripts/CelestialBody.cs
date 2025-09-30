using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    public float surfaceG = 1.0f;
    public Vector3 initialVelocity;
    public Vector3 velocity;

    private Rigidbody rb;
    public float radius => this.transform.localScale.x * 0.5f;
    public float mass => this.surfaceG * this.radius * this.radius;

    private void Awake()
    {
        this.velocity = this.initialVelocity;
        this.rb = this.GetComponent<Rigidbody>();
    }

    public void UpdateVelocity(ref CelestialBody[] others)
    {
        foreach (var o in others)
        {
            if (o != this)
            {
                Vector3 disp = o.rb.position - this.rb.position;
                Vector3 acc = disp.normalized * o.mass / disp.sqrMagnitude;
                this.velocity += acc * Time.fixedDeltaTime;
            }
        }
    }

    public void UpdatePosition()
    {
        this.rb.position += this.velocity * Time.fixedDeltaTime;
    }
}
