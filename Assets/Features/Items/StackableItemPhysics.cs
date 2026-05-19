using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StackableItemPhysics : MonoBehaviour
{
    [Header("Fall Detection")]
    [SerializeField] private float fallYThreshold = -1f;
    [SerializeField] private float sideFallMargin = 0.5f;

    [Header("Anti Slide")]
    [SerializeField] private float maxAngleForSideBrake = 5f;

    [Tooltip("Seitliche Bewegungen unter dieser Geschwindigkeit werden abgebremst.")]
    [SerializeField] private float minSideSpeedToSlide = 0.45f;

    [Tooltip("Wie stark kleine seitliche Bewegungen abgebremst werden.")]
    [SerializeField] private float sideBrake = 16f;

    private Rigidbody rb;
    private Carriage carriage;
    private bool hasFallen;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Carriage carriage)
    {
        this.carriage = carriage;

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        rb.constraints =
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY;
    }

    private void FixedUpdate()
    {
        StopSmallSideMovement();
    }

    private void Update()
    {
        if (hasFallen || carriage == null)
            return;

        if (!IsOffCarriage())
            return;

        hasFallen = true;
        Debug.Log($"{name} fell from the carriage.");
        Destroy(gameObject, 2f);
    }

    private void StopSmallSideMovement()
    {
        float angle = Mathf.Abs(Mathf.DeltaAngle(0f, transform.eulerAngles.z));
        float sideSpeed = Mathf.Abs(rb.linearVelocity.x);

        bool isNotTiltedTooMuch = angle < maxAngleForSideBrake;
        bool isMovingTooSlowlyToSlide = sideSpeed < minSideSpeedToSlide;

        if (!isNotTiltedTooMuch || !isMovingTooSlowlyToSlide)
            return;

        Vector3 velocity = rb.linearVelocity;

        velocity.x = Mathf.MoveTowards(
            velocity.x,
            0f,
            sideBrake * Time.fixedDeltaTime
        );

        rb.linearVelocity = velocity;
    }

    private bool IsOffCarriage()
    {
        Vector3 localPosition = carriage.transform.InverseTransformPoint(transform.position);
        float halfWidth = carriage.GetCarriageWidthInWorldUnits() * 0.5f;

        bool fellDown = localPosition.y < fallYThreshold;
        bool fellSideways = Mathf.Abs(localPosition.x) > halfWidth + sideFallMargin;

        return fellDown || fellSideways;
    }
}