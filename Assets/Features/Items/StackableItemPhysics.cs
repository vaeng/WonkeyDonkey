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
    [SerializeField] private float centerOfMassEdgeMargin = 0.03f;
    [SerializeField] private float wobbleEdgeDistance = 0.15f;
    [SerializeField] private float supportCheckDepth = 0.12f;
    [SerializeField] private float aboveItemTolerance = 0.15f;
    [SerializeField] private LayerMask supportLayers;

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

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs;
    [SerializeField] private bool drawDebug;

    private Rigidbody rb;
    private Collider itemCollider;
    private Carriage carriage;

    private bool hasFallen;
    private bool isUnstable;
    private bool isWobbling;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponentInChildren<Collider>();
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

        CheckIfBalanced();
        ReactToBalanceState();
    }

    private void Update()
    {
        if (hasFallen || carriage == null)
            return;

        if (!IsOffCarriage())
            return;

        hasFallen = true;

        if (showDebugLogs)
            Debug.Log($"{name} fell from the carriage.");

        Destroy(gameObject, 2f);
    }

    private void CheckIfBalanced()
    {
        Bounds bounds = itemCollider.bounds;

        if (!TryGetSupportArea(bounds, out float supportLeft, out float supportRight))
        {
            isUnstable = true;
            isWobbling = false;
            return;
        }

        float centerX = GetCenterOfMassWithWeightAbove(bounds);

        float distanceToLeftEdge = centerX - supportLeft;
        float distanceToRightEdge = supportRight - centerX;
        float closestEdgeDistance = Mathf.Min(distanceToLeftEdge, distanceToRightEdge);

        isUnstable = closestEdgeDistance <= centerOfMassEdgeMargin;
        isWobbling = !isUnstable && closestEdgeDistance <= wobbleEdgeDistance;

        if (drawDebug)
            DrawDebugLines(bounds, supportLeft, supportRight, centerX);

        if (showDebugLogs && (isUnstable || isWobbling))
        {
            string state = isUnstable ? "unstable" : "wobbling";

            Debug.Log(
                $"{name} is {state}. " +
                $"Support: {supportLeft:0.00} to {supportRight:0.00}, " +
                $"Center: {centerX:0.00}, " +
                $"Edge Distance: {closestEdgeDistance:0.00}"
            );
        }
    }

    private void ReactToBalanceState()
    {
        if (isUnstable)
        {
            FreezeZRotation(false);
            PushTowardsFallDirection();
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

    private bool TryGetSupportArea(Bounds bounds, out float supportLeft, out float supportRight)
    {
        supportLeft = float.MaxValue;
        supportRight = float.MinValue;

        Vector3 checkCenter = new Vector3(
            bounds.center.x,
            bounds.min.y - supportCheckDepth * 0.5f,
            bounds.center.z
        );

        Vector3 checkSize = new Vector3(
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

            Bounds supportBounds = hit.bounds;

            float left = Mathf.Max(bounds.min.x, supportBounds.min.x);
            float right = Mathf.Min(bounds.max.x, supportBounds.max.x);

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

        bool isBelowItem = hit.bounds.max.y <= ownBounds.min.y + supportCheckDepth * 2f;

        return isBelowItem;
    }

    private float GetCenterOfMassWithWeightAbove(Bounds ownBounds)
    {
        float totalMass = rb.mass;
        float weightedX = rb.worldCenterOfMass.x * rb.mass;

        foreach (StackableItemPhysics item in allItems)
        {
            if (!IsWeightAbove(item, ownBounds))
                continue;

            float itemMass = item.rb.mass;

            totalMass += itemMass;
            weightedX += item.rb.worldCenterOfMass.x * itemMass;
        }

        return weightedX / totalMass;
    }

    private bool IsWeightAbove(StackableItemPhysics otherItem, Bounds ownBounds)
    {
        if (otherItem == null || otherItem == this)
            return false;

        if (otherItem.rb == null || otherItem.itemCollider == null)
            return false;

        Bounds otherBounds = otherItem.itemCollider.bounds;

        bool isAbove = otherBounds.min.y >= ownBounds.max.y - aboveItemTolerance;

        bool overlapsX =
            otherBounds.max.x > ownBounds.min.x &&
            otherBounds.min.x < ownBounds.max.x;

        bool overlapsZ =
            otherBounds.max.z > ownBounds.min.z &&
            otherBounds.min.z < ownBounds.max.z;

        return isAbove && overlapsX && overlapsZ;
    }

    private void StandStraightIfPossible()
    {
        float angle = Mathf.Abs(Mathf.DeltaAngle(0f, rb.rotation.eulerAngles.z));

        if (angle <= angleToFreezeAsStraight)
        {
            SetStraightRotation();
            FreezeZRotation(true);
            return;
        }

        if (angle <= maxAngleToAutoUpright)
        {
            FreezeZRotation(false);
            RotateBackToStraight();
            return;
        }

        // Zu schief soll nicht einfach magisch zurückgesetzt werden.
        FreezeZRotation(false);
    }

    private void FreezeZRotation(bool shouldFreeze)
    {
        rb.constraints =
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY;

        if (shouldFreeze)
            rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
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

        float newZ = Mathf.MoveTowardsAngle(
            euler.z,
            0f,
            uprightSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(Quaternion.Euler(euler.x, euler.y, newZ));

        Vector3 angularVelocity = rb.angularVelocity;
        angularVelocity.z = Mathf.MoveTowards(
            angularVelocity.z,
            0f,
            uprightAngularBrake * Time.fixedDeltaTime
        );

        rb.angularVelocity = angularVelocity;
    }

    private void SetStraightRotation()
    {
        Vector3 euler = rb.rotation.eulerAngles;

        rb.MoveRotation(Quaternion.Euler(euler.x, euler.y, 0f));

        Vector3 angularVelocity = rb.angularVelocity;
        angularVelocity.z = 0f;
        rb.angularVelocity = angularVelocity;
    }

    private void AddWobble()
    {
        float wobble = Mathf.Sin(Time.time * 12f) * wobbleTorque;

        rb.AddTorque(
            Vector3.forward * wobble,
            ForceMode.VelocityChange
        );
    }

    private void PushTowardsFallDirection()
    {
        Bounds bounds = itemCollider.bounds;
        float centerX = GetCenterOfMassWithWeightAbove(bounds);

        float direction = centerX >= bounds.center.x ? -1f : 1f;

        rb.AddTorque(
            Vector3.forward * direction * 0.18f,
            ForceMode.VelocityChange
        );
    }

    private bool IsOffCarriage()
    {
        Vector3 localPosition = carriage.transform.InverseTransformPoint(transform.position);
        float halfWidth = carriage.GetCarriageWidthInWorldUnits() * 0.5f;

        bool fellDown = localPosition.y < fallYThreshold;
        bool fellSideways = Mathf.Abs(localPosition.x) > halfWidth + sideFallMargin;

        return fellDown || fellSideways;
    }

    private void DrawDebugLines(Bounds bounds, float supportLeft, float supportRight, float centerX)
    {
        Color supportColor = Color.green;

        if (isUnstable)
            supportColor = Color.red;
        else if (isWobbling)
            supportColor = Color.yellow;

        Vector3 supportStart = new Vector3(supportLeft, bounds.min.y - 0.05f, bounds.center.z);
        Vector3 supportEnd = new Vector3(supportRight, bounds.min.y - 0.05f, bounds.center.z);

        Debug.DrawLine(supportStart, supportEnd, supportColor);

        Vector3 centerStart = new Vector3(centerX, bounds.min.y, bounds.center.z);
        Vector3 centerEnd = new Vector3(centerX, bounds.max.y + 0.5f, bounds.center.z);

        Debug.DrawLine(centerStart, centerEnd, Color.cyan);
    }
}