using UnityEngine;

public class OrbitalCam : MonoBehaviour
{
    //public CelestialBody axis;
    public CelestialBody target;
    [Min(0)] public float distance = 10.0f;

    private void Awake()
    {
        //this.transform.parent = this.target.transform;
    }

    private void LateUpdate()
    {
        this.transform.SetPositionAndRotation(
            this.target.transform.position + Vector3.up * distance,
            Quaternion.Euler(90, 0, 0)
        );
        //if (this.axis == null || this.target == null)
        //var disp = this.target.transform.position - this.axis.transform.position;
        //var up = Vector3.Cross(this.target.velocity, disp).normalized;

        //this.transform.SetPositionAndRotation(
        //    this.target.transform.position + up * distance,
        //    Quaternion.LookRotation(this.target.transform.position - this.transform.position, Vector3.up)
        //    //Quaternion.Euler(-90, 0, 0)
        //);
    }
}
