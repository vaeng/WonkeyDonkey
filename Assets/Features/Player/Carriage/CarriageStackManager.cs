using UnityEngine;

/// <summary>
/// Places collected items on the carriage as physical stackable objects.
/// </summary>
public class CarriageStackManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Carriage carriage;
    [SerializeField] private Transform stackRoot;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnBottomOffset = 0.05f;
    [SerializeField] private float raycastStartHeight = 10f;
    [SerializeField] private float raycastDistance = 20f;
    [SerializeField] private float itemDepth = 0.8f;
    [SerializeField] private bool scaleStackedItemsFromData = false;

    [Header("Physics Settings")]
    [SerializeField] private float linearDamping = 0.2f;
    [SerializeField] private float angularDamping = 0.8f;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask stackSurfaceLayers = ~0;

    public void PlaceCollectedItem(
        CollectableItem collectedItem,
        int carriageSpotIndex
    )
    {
        if (collectedItem == null)
        {
            Debug.LogWarning("Cannot place collected item because item is null.");
            return;
        }

        if (collectedItem.StackedPrefab == null)
        {
            Debug.LogWarning(
                "Collected item has no stacked prefab assigned: " +
                collectedItem.name
            );
            return;
        }

        Vector3 localSpotPosition =
            carriage.GetLocalPositionForSpot(carriageSpotIndex);

        Vector3 localSpawnBasePosition = new Vector3(
            localSpotPosition.x,
            0f,
            0f
        );

        Vector3 worldSpawnBasePosition =
            stackRoot.TransformPoint(localSpawnBasePosition);

        GameObject stackedObject = Instantiate(
            collectedItem.StackedPrefab,
            worldSpawnBasePosition,
            stackRoot.rotation
        );

        RemoveWorldMoverIfPresent(stackedObject);

        if (scaleStackedItemsFromData)
        {
            stackedObject.transform.localScale = new Vector3(
                collectedItem.WidthInLanes,
                collectedItem.HeightInLanes,
                itemDepth
            );
        }

        MoveItemBottomAboveHighestSurface(
            stackedObject,
            worldSpawnBasePosition,
            collectedItem
        );

        SetupStackedItemPhysics(stackedObject, collectedItem);

        Debug.Log(
            "Placed " +
            collectedItem.ItemType +
            " on carriage spot " +
            carriageSpotIndex
        );
    }

    private void RemoveWorldMoverIfPresent(GameObject stackedObject)
    {
        WorldMover worldMover = stackedObject.GetComponent<WorldMover>();

        if (worldMover != null)
            Destroy(worldMover);
    }

    private void MoveItemBottomAboveHighestSurface(GameObject stackedObject, Vector3 worldSpawnBasePosition, CollectableItem collectedItem)
        {
            Physics.SyncTransforms();

            Collider itemCollider =
                stackedObject.GetComponentInChildren<Collider>();

            if (itemCollider == null)
            {
                Debug.LogWarning(
                    "Stacked object has no collider: " +
                    stackedObject.name
                );
                return;
            }

            bool previousColliderState = itemCollider.enabled;
            itemCollider.enabled = false;

            Physics.SyncTransforms();

            float itemWidthInWorldUnits =
                collectedItem.WidthInLanes * carriage.GetLaneWidth();

            float surfaceY =
                GetHighestSurfaceYBelowItemFootprint(
                    worldSpawnBasePosition,
                    itemWidthInWorldUnits
                );

            itemCollider.enabled = previousColliderState;

            Physics.SyncTransforms();

            float desiredBottomY =
                surfaceY + spawnBottomOffset;

            float currentBottomY =
                itemCollider.bounds.min.y;

            float yCorrection =
                desiredBottomY - currentBottomY;

            stackedObject.transform.position += Vector3.up * yCorrection;

            Physics.SyncTransforms();
        }


    private float GetHighestSurfaceYBelowItemFootprint(
        Vector3 worldCenterPosition,
        float itemWidthInWorldUnits
    )
        {
            float highestSurfaceY = float.MinValue;

            float halfWidth = itemWidthInWorldUnits * 0.5f;

            Vector3[] sampleOffsets =
            {
            new Vector3(-halfWidth * 0.45f, 0f, 0f),
            Vector3.zero,
            new Vector3(halfWidth * 0.45f, 0f, 0f)
        };

            foreach (Vector3 offset in sampleOffsets)
            {
                Vector3 samplePosition =
                    worldCenterPosition + carriage.transform.TransformDirection(offset);

                Vector3 rayStart = new Vector3(
                    samplePosition.x,
                    stackRoot.position.y + raycastStartHeight,
                    samplePosition.z
                );

                bool hitSomething = Physics.Raycast(
                    rayStart,
                    Vector3.down,
                    out RaycastHit hit,
                    raycastDistance,
                    stackSurfaceLayers,
                    QueryTriggerInteraction.Ignore
                );

                if (!hitSomething)
                    continue;

                if (hit.point.y > highestSurfaceY)
                    highestSurfaceY = hit.point.y;
            }

            if (highestSurfaceY != float.MinValue)
                return highestSurfaceY;

            Debug.LogWarning(
                "No stack surface found below item footprint. " +
                "Using StackSpawnRoot height instead."
            );

            return stackRoot.position.y;
        }


    private void SetupStackedItemPhysics(
        GameObject stackedObject,
        CollectableItem collectedItem
    )
    {
        Rigidbody rb = stackedObject.GetComponent<Rigidbody>();

        if (rb == null)
            rb = stackedObject.AddComponent<Rigidbody>();

        rb.mass = collectedItem.Mass;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        rb.linearDamping = linearDamping;
        rb.angularDamping = angularDamping;

        rb.constraints =
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY;

        StackableItemPhysics stackable =
            stackedObject.GetComponent<StackableItemPhysics>();

        if (stackable == null)
            stackable = stackedObject.AddComponent<StackableItemPhysics>();

        stackable.Initialize(carriage);
    }
}