using KinematicCharacterController;
using UnityEngine;

public class Player : MonoBehaviour, ICharacterController
{
    struct PlayerInputs
    {
        public float MoveAxisForward;
        public float MoveAxisRight;
        public Quaternion CameraRotation;
        public bool JumpDown;
    }

    public KinematicCharacterMotor Motor;
    public Camera Cam;
    [Range(0.5f, 10f)] public float MouseSensitivity = 2.5f;
    [Range(0f, 20f)] public float WalkSpeed = 4f;
    [Range(0f, 20f)] public float JumpSpeed = 8f;

    private float camPitch;
    private float camYaw;
    private PlayerInputs inputs;

    private void Awake()
    {
        this.Motor.CharacterController = this;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        this.inputs = new PlayerInputs
        {
            MoveAxisForward = Input.GetAxisRaw("Vertical"),
            MoveAxisRight = Input.GetAxisRaw("Horizontal"),
            CameraRotation = Cam.transform.rotation,
            JumpDown = Input.GetKeyDown(KeyCode.Space)
        };
    }

    private void LateUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        this.camYaw += mouseX * MouseSensitivity;
        this.camPitch -= mouseY * MouseSensitivity;
        this.camPitch = Mathf.Clamp(camPitch, -90f, 90f);
        this.Cam.transform.SetPositionAndRotation(
            this.transform.position + Vector3.up * 0.5f,
            Quaternion.Euler(camPitch, camYaw, 0f)
        );
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true; // collide with everything for now
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void PostGroundingUpdate(float deltaTime)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        currentRotation = Quaternion.Euler(0f, this.camYaw, 0f);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (this.Motor.GroundingStatus.IsStableOnGround)
        {
            Vector3 inputDir = new(this.inputs.MoveAxisRight, 0f, this.inputs.MoveAxisForward);
            Vector3 disp = this.transform.TransformDirection(inputDir.normalized);
            currentVelocity = disp * this.WalkSpeed;

            // Handle jumping
            if (this.inputs.JumpDown)
            {
                Vector3 jumpDirection = this.Motor.CharacterUp;
                if (this.Motor.GroundingStatus.FoundAnyGround && !this.Motor.GroundingStatus.IsStableOnGround)
                {
                    jumpDirection = this.Motor.GroundingStatus.GroundNormal;
                }

                this.Motor.ForceUnground();

                currentVelocity += jumpDirection * this.JumpSpeed;

                this.inputs.JumpDown = false;
            }
        }
        else
        {
            currentVelocity += Physics.gravity * deltaTime;
        }
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }
}
