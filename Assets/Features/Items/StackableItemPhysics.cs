using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StackableItemPhysics : MonoBehaviour
{
    [Header("Fall Detection")]
    [SerializeField] private float fallYThreshold = -1f;
    [SerializeField] private float sideFallMargin = 0.5f;

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

    private bool IsOffCarriage()
    {
        Vector3 localPosition = carriage.transform.InverseTransformPoint(transform.position);
        float halfWidth = carriage.GetCarriageWidthInWorldUnits() * 0.5f;

        bool fellDown = localPosition.y < fallYThreshold;
        bool fellSideways = Mathf.Abs(localPosition.x) > halfWidth + sideFallMargin;

        return fellDown || fellSideways;
    }
}