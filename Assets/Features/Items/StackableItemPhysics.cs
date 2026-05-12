using UnityEngine;

/// <summary>
/// Handles simple carriage-relative physics for stacked items.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class StackableItemPhysics : MonoBehaviour
{
    [Header("Carriage Following")]
    [SerializeField] private bool followCarriageSideMovement = true;

    [Header("Fall Detection")]
    [SerializeField] private float fallYThreshold = -1f;
    [SerializeField] private float sideFallMargin = 0.5f;

    private Rigidbody rb;
    private Carriage carriage;
    private Transform carriageTransform;

    private Vector3 previousCarriagePosition;

    private bool isInitialized;
    private bool hasFallen;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Carriage carriage)
    {
        this.carriage = carriage;
        carriageTransform = carriage.transform;

        previousCarriagePosition = carriageTransform.position;

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        rb.constraints =
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY;

        isInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!isInitialized || hasFallen || carriageTransform == null)
            return;

        if (followCarriageSideMovement)
            FollowCarriageSideMovement();

        previousCarriagePosition = carriageTransform.position;
    }

    private void Update()
    {
        if (!isInitialized || hasFallen || carriage == null)
            return;

        if (HasFallenFromCarriage())
        {
            hasFallen = true;

            Debug.Log(gameObject.name + " fell from carriage.");

            Destroy(gameObject, 2f);
        }
    }

    private void FollowCarriageSideMovement()
    {
        Vector3 carriageDelta =
            carriageTransform.position - previousCarriagePosition;

        Vector3 sideDelta = new Vector3(
            carriageDelta.x,
            0f,
            0f
        );

        rb.MovePosition(rb.position + sideDelta);
    }

    private bool HasFallenFromCarriage()
    {
        Vector3 localPosition =
            carriage.transform.InverseTransformPoint(transform.position);

        float halfCarriageWidth =
            carriage.GetCarriageWidthInWorldUnits() * 0.5f;

        bool fellBelowCarriage =
            localPosition.y < fallYThreshold;

        bool fellSideways =
            Mathf.Abs(localPosition.x) >
            halfCarriageWidth + sideFallMargin;

        return fellBelowCarriage || fellSideways;
    }
}