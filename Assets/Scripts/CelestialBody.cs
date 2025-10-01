using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    public float surfaceG = 1.0f;
    public Vector3 initialVelocity;

    [Min(0f)]
    [SerializeField]
    private float radius = 10f;
    public float Radius => this.radius;
    public float Mass => this.surfaceG * this.radius * this.radius;

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
}
