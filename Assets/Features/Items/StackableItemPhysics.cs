using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StackableItemPhysics : MonoBehaviour
{
    [Header("Carriage Following")]
    [SerializeField] private bool followCarriageMovement = true;
    [SerializeField] private bool lockLocalZPosition = true;
    [SerializeField] private bool lockForwardBackwardRotation = true;

    [Header("Fall Detection")]
    [SerializeField] private float fallYThreshold = -1f;
    [SerializeField] private float sideFallMargin = 0.5f;

    private Rigidbody rb;
    private Carriage carriage;
    private Transform carriageTransform;

    private Vector3 previousCarriagePosition;
    private float lockedLocalZ;

    private bool isInitialized;
    private bool hasFallen;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Carriage carriage, float lockedLocalZ)
    {
        this.carriage = carriage;
        this.lockedLocalZ = lockedLocalZ;

        carriageTransform = carriage.transform;
        previousCarriagePosition = carriageTransform.position;

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY;

        isInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!isInitialized || hasFallen || carriageTransform == null)
            return;

        Vector3 targetPosition = rb.position;

        if (followCarriageMovement)
        {
            Vector3 carriageDelta =
                carriageTransform.position - previousCarriagePosition;

            targetPosition.x += carriageDelta.x;
        }

        //if (lockLocalZPosition)
        //{
        //    targetPosition = GetPositionWithLockedLocalZ(targetPosition);
        //    RemoveLocalZVelocity();
        //}

        if (lockForwardBackwardRotation)
        {
            RemoveForwardBackwardAngularVelocity();
            LockForwardBackwardRotation();
        }

        rb.position = targetPosition;
        Physics.SyncTransforms();

        previousCarriagePosition = carriageTransform.position;
    }

    private void LateUpdate()
    {
        if (!isInitialized || hasFallen || carriageTransform == null)
            return;

        if (!lockLocalZPosition)
            return;

        Vector3 correctedPosition = GetPositionWithLockedLocalZ(rb.position);

        rb.position = correctedPosition;

        Vector3 localVelocity =
            carriageTransform.InverseTransformDirection(rb.linearVelocity);

        localVelocity.z = 0f;

        rb.linearVelocity =
            carriageTransform.TransformDirection(localVelocity);
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

    private Vector3 GetPositionWithLockedLocalZ(Vector3 worldPosition)
    {
        Vector3 localPosition =
            carriageTransform.InverseTransformPoint(worldPosition);

        localPosition.z = lockedLocalZ;

        return carriageTransform.TransformPoint(localPosition);
    }

    private void RemoveLocalZVelocity()
    {
        Vector3 localVelocity =
            carriageTransform.InverseTransformDirection(rb.linearVelocity);

        localVelocity.z = 0f;

        rb.linearVelocity =
            carriageTransform.TransformDirection(localVelocity);
    }

    private void RemoveForwardBackwardAngularVelocity()
    {
        Vector3 localAngularVelocity =
            carriageTransform.InverseTransformDirection(rb.angularVelocity);

        localAngularVelocity.x = 0f;
        localAngularVelocity.y = 0f;

        rb.angularVelocity =
            carriageTransform.TransformDirection(localAngularVelocity);
    }

    private void LockForwardBackwardRotation()
    {
        Vector3 localEuler =
            (Quaternion.Inverse(carriageTransform.rotation) * rb.rotation).eulerAngles;

        localEuler.x = 0f;
        localEuler.y = 0f;

        Quaternion correctedLocalRotation =
            Quaternion.Euler(localEuler);

        Quaternion correctedWorldRotation =
            carriageTransform.rotation * correctedLocalRotation;

        rb.MoveRotation(correctedWorldRotation);
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