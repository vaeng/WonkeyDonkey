using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StackableItemPhysics : MonoBehaviour
{
    private static readonly List<StackableItemPhysics> allItems = new();

    [Header("Fall Detection")]
    [SerializeField] private float fallYThreshold = -1f;
    [SerializeField] private float sideFallMargin = 0.5f;

    [Header("Balance")]
    [SerializeField] private float centerOfMassEdgeMargin = 0.04f;
    [SerializeField] private float wobbleEdgeDistance = 0.15f;
    [SerializeField] private float minOverhangToTip = 0.35f;
    [SerializeField] private float supportCheckDepth = 0.12f;
    [SerializeField] private float aboveItemTolerance = 0.15f;
    [SerializeField] private LayerMask supportLayers;

    [Header("Tipping")]
    [SerializeField] private float tippingTorque = 0.7f;
    [SerializeField] private float tippingVelocityTorque = 0.25f;
    [SerializeField] private float maxTippingAngularSpeed = 4f;
    [SerializeField] private float slideBrakeWhileTipping = 12f;

    [Header("Stabilizing")]
    [SerializeField] private float maxAngleToStabilize = 8f;
    [SerializeField] private float sideBrake = 20f;
    [SerializeField] private float rotationBrake = 20f;
    [SerializeField] private float wobbleTorque = 0.08f;

    [Header("Auto Upright")]
    [SerializeField] private float maxAngleToAutoUpright = 12f;
    [SerializeField] private float angleToFreezeAsStraight = 0.5f;
    [SerializeField] private float uprightSpeed = 90f;
    [SerializeField] private float uprightAngularBrake = 25f;

    public static System.Action OnItemFallen;

    private Rigidbody rb;
    private Collider collider;
    private Collider itemCollider;
    private Carriage carriage;

    private bool hasFallen;
    private bool isWobbling;
    private float fallDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponentInChildren<Collider>();
        collider = GetComponentInChildren<BoxCollider>();
        if (collider == null)
        {
            collider = GetComponent<SphereCollider>();
        }
    }

    private void OnEnable()
    {
        if (!allItems.Contains(this))
            allItems.Add(this);
    }

    private void OnDisable()
    {
        allItems.Remove(this);
    }

    public void Initialize(Carriage carriage)
    {
        this.carriage = carriage;

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        FreezeZRotation(true);
    }

    private void FixedUpdate()
    {
        if (hasFallen || carriage == null || itemCollider == null)
            return;

        CheckBalance();

        if (fallDirection != 0f)
        {
            FreezeZRotation(false);
            BrakeSideMovement();
            TipOver();
            return;
        }

        if (isWobbling)
        {
            FreezeZRotation(false);
            SlowDownSmallMovement();
            AddWobble();
            return;
        }

        StandStraightIfPossible();
        SlowDownSmallMovement();
    }

    private void Update()
    {
        float heightToDespawn = collider switch
        {
            BoxCollider box => (box.size.y / 2f) + 0.1f,
            SphereCollider sphere => sphere.radius + 0.1f,
            CapsuleCollider capsule => (capsule.height / 2f) + 0.1f,
            MeshCollider mesh => mesh.bounds.extents.y + 0.1f,
            _ => collider.bounds.extents.y + 0.1f
        };
        if (transform.position.y < heightToDespawn)
        {
            OnItemFallen?.Invoke();
            Destroy(gameObject);
        }
    }

    private void CheckBalance()
    {
        Bounds bounds = itemCollider.bounds;

        fallDirection = 0f;
        isWobbling = false;

        if (!FindSupport(bounds, out float supportLeft, out float supportRight))
        {
            fallDirection = GetFallDirection(bounds);
            return;
        }

        float centerX = GetWeightedCenterX(bounds);

        float leftDistance = centerX - supportLeft;
        float rightDistance = supportRight - centerX;

        float leftOverhang = Mathf.Max(0f, supportLeft - bounds.min.x);
        float rightOverhang = Mathf.Max(0f, bounds.max.x - supportRight);

        float maxOverhang = bounds.size.x * minOverhangToTip;

        if (leftDistance <= centerOfMassEdgeMargin || leftOverhang >= maxOverhang)
            fallDirection = 1f;
        else if (rightDistance <= centerOfMassEdgeMargin || rightOverhang >= maxOverhang)
            fallDirection = -1f;
        else
            isWobbling = Mathf.Min(leftDistance, rightDistance) <= wobbleEdgeDistance;
    }

    private bool FindSupport(Bounds bounds, out float supportLeft, out float supportRight)
    {
        supportLeft = float.MaxValue;
        supportRight = float.MinValue;

        Vector3 checkCenter = new(
            bounds.center.x,
            bounds.min.y - supportCheckDepth * 0.5f,
            bounds.center.z
        );

        Vector3 checkSize = new(
            bounds.extents.x * 0.98f,
            supportCheckDepth * 0.5f,
            bounds.extents.z * 0.98f
        );

        Collider[] hits = Physics.OverlapBox(
            checkCenter,
            checkSize,
            Quaternion.identity,
            supportLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            if (!CanBeSupport(hit, bounds))
                continue;

            float left = Mathf.Max(bounds.min.x, hit.bounds.min.x);
            float right = Mathf.Min(bounds.max.x, hit.bounds.max.x);

            if (right <= left)
                continue;

            supportLeft = Mathf.Min(supportLeft, left);
            supportRight = Mathf.Max(supportRight, right);
        }

        return supportLeft != float.MaxValue;
    }

    private bool CanBeSupport(Collider hit, Bounds ownBounds)
    {
        if (hit == null || hit == itemCollider)
            return false;

        if (hit.transform.IsChildOf(transform))
            return false;

        return hit.bounds.max.y <= ownBounds.min.y + supportCheckDepth * 2f;
    }

    private float GetWeightedCenterX(Bounds ownBounds)
    {
        float mass = rb.mass;
        float x = rb.worldCenterOfMass.x * rb.mass;

        foreach (StackableItemPhysics item in allItems)
        {
            if (!IsOnTopOfMe(item, ownBounds))
                continue;

            mass += item.rb.mass;
            x += item.rb.worldCenterOfMass.x * item.rb.mass;
        }

        return x / mass;
    }

    private bool IsOnTopOfMe(StackableItemPhysics item, Bounds ownBounds)
    {
        if (item == null || item == this || item.rb == null || item.itemCollider == null)
            return false;

        Bounds otherBounds = item.itemCollider.bounds;

        bool aboveMe = otherBounds.min.y >= ownBounds.max.y - aboveItemTolerance;
        bool sameXArea = otherBounds.max.x > ownBounds.min.x && otherBounds.min.x < ownBounds.max.x;
        bool sameZArea = otherBounds.max.z > ownBounds.min.z && otherBounds.min.z < ownBounds.max.z;

        return aboveMe && sameXArea && sameZArea;
    }

    private void TipOver()
    {
        if (Mathf.Abs(rb.linearVelocity.x) > 0.05f)
            fallDirection = rb.linearVelocity.x > 0f ? -1f : 1f;

        rb.AddTorque(Vector3.forward * fallDirection * tippingTorque, ForceMode.Acceleration);

        float extraTorque = Mathf.Abs(rb.linearVelocity.x) * tippingVelocityTorque;
        rb.AddTorque(Vector3.forward * fallDirection * extraTorque, ForceMode.Acceleration);

        Vector3 angularVelocity = rb.angularVelocity;
        angularVelocity.z = Mathf.Clamp(angularVelocity.z, -maxTippingAngularSpeed, maxTippingAngularSpeed);
        rb.angularVelocity = angularVelocity;
    }

    private void BrakeSideMovement()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.x = Mathf.MoveTowards(velocity.x, 0f, slideBrakeWhileTipping * Time.fixedDeltaTime);
        rb.linearVelocity = velocity;
    }

    private float GetFallDirection(Bounds bounds)
    {
        if (Mathf.Abs(rb.linearVelocity.x) > 0.05f)
            return rb.linearVelocity.x > 0f ? -1f : 1f;

        return bounds.center.x >= carriage.transform.position.x ? -1f : 1f;
    }

    private void StandStraightIfPossible()
    {
        float angle = Mathf.Abs(Mathf.DeltaAngle(0f, rb.rotation.eulerAngles.z));

        if (angle <= angleToFreezeAsStraight)
        {
            SnapStraight();
            FreezeZRotation(true);
            return;
        }

        FreezeZRotation(false);

        if (angle <= maxAngleToAutoUpright)
            RotateBackToStraight();
    }

    private void SlowDownSmallMovement()
    {
        float angle = Mathf.Abs(Mathf.DeltaAngle(0f, rb.rotation.eulerAngles.z));

        if (angle > maxAngleToStabilize)
            return;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = Mathf.MoveTowards(velocity.x, 0f, sideBrake * Time.fixedDeltaTime);
        rb.linearVelocity = velocity;

        Vector3 angularVelocity = rb.angularVelocity;
        angularVelocity.z = Mathf.MoveTowards(angularVelocity.z, 0f, rotationBrake * Time.fixedDeltaTime);
        rb.angularVelocity = angularVelocity;
    }

    private void RotateBackToStraight()
    {
        Vector3 euler = rb.rotation.eulerAngles;

        euler.z = Mathf.MoveTowardsAngle(
            euler.z,
            0f,
            uprightSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(Quaternion.Euler(euler));

        Vector3 angularVelocity = rb.angularVelocity;
        angularVelocity.z = Mathf.MoveTowards(
            angularVelocity.z,
            0f,
            uprightAngularBrake * Time.fixedDeltaTime
        );

        rb.angularVelocity = angularVelocity;
    }

    private void SnapStraight()
    {
        Vector3 euler = rb.rotation.eulerAngles;
        euler.z = 0f;

        rb.MoveRotation(Quaternion.Euler(euler));

        Vector3 angularVelocity = rb.angularVelocity;
        angularVelocity.z = 0f;
        rb.angularVelocity = angularVelocity;
    }

    private void AddWobble()
    {
        float wobble = Mathf.Sin(Time.time * 12f) * wobbleTorque;
        rb.AddTorque(Vector3.forward * wobble, ForceMode.VelocityChange);
    }

    private void FreezeZRotation(bool freezeZ)
    {
        rb.constraints =
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY;

        if (freezeZ)
            rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
    }

    private bool IsOffCarriage()
    {
        Vector3 localPosition = carriage.transform.InverseTransformPoint(transform.position);
        float maxX = carriage.GetCarriageWidthInWorldUnits() * 0.5f + sideFallMargin;

        return localPosition.y < fallYThreshold || Mathf.Abs(localPosition.x) > maxX;
    }
}