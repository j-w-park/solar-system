using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    public float surfaceG = 1.0f;
    public Vector3 initialVelocity;
    public Vector3 velocity;

    [Min(0f)]
    [SerializeField]
    private float radius = 10f;
    public float Radius => this.radius;
    public float Mass => this.surfaceG * this.radius * this.radius;

    private Rigidbody rb;

    private void Awake()
    {
        this.velocity = this.initialVelocity;
        this.rb = this.GetComponent<Rigidbody>();
    }

    private void OnValidate()
    {
        float d = Mathf.Max(0f, 2f * this.radius);
        this.transform.localScale = new Vector3(d, d, d);
    }

    private void Reset()
    {
        this.radius = Mathf.Max(0f, this.transform.localScale.x * 0.5f);
        float d = 2f * this.radius;
        this.transform.localScale = new Vector3(d, d, d);
    }

    public void UpdateVelocity(ref CelestialBody[] others)
    {
        foreach (var o in others)
        {
            if (o != this)
            {
                Vector3 disp = o.rb.position - this.rb.position;
                Vector3 acc = disp.normalized * o.Mass / disp.sqrMagnitude;
                this.velocity += acc * Time.fixedDeltaTime;
            }
        }
    }

    public void UpdatePosition()
    {
        this.rb.position += this.velocity * Time.fixedDeltaTime;
    }
}
