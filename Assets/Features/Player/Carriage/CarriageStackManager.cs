using UnityEngine;

public class CarriageStackManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Carriage carriage;
    [SerializeField] private Transform stackRoot;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnBottomOffset = 0.15f;
    [SerializeField] private float raycastStartHeight = 10f;
    [SerializeField] private float raycastDistance = 20f;
    [SerializeField] private float itemDepth = 0.8f;
    [SerializeField] private bool scaleStackedItemsFromData = false;

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

        if (scaleStackedItemsFromData)
        {
            stackedObject.transform.localScale = new Vector3(
                collectedItem.WidthInLanes,
                collectedItem.HeightInLanes,
                itemDepth
            );
        }

        MoveItemBottomAboveHighestSurface(stackedObject, worldSpawnBasePosition);

        SetupStackedItemPhysics(stackedObject, collectedItem);

        Debug.Log(
            "Placed " +
            collectedItem.ItemType +
            " on carriage spot " +
            carriageSpotIndex
        );
    }

    private void MoveItemBottomAboveHighestSurface(
        GameObject stackedObject,
        Vector3 worldSpawnBasePosition
    )
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

        // Disable the new item's collider during the raycast
        // so the raycast cannot hit the newly spawned item itself.
        bool previousColliderState = itemCollider.enabled;
        itemCollider.enabled = false;

        Physics.SyncTransforms();

        float surfaceY =
            GetHighestSurfaceYAtPosition(worldSpawnBasePosition);

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

    private float GetHighestSurfaceYAtPosition(Vector3 worldPosition)
    {
        Vector3 rayStart = new Vector3(
            worldPosition.x,
            stackRoot.position.y + raycastStartHeight,
            worldPosition.z
        );

        bool hitSomething = Physics.Raycast(
            rayStart,
            Vector3.down,
            out RaycastHit hit,
            raycastDistance,
            stackSurfaceLayers,
            QueryTriggerInteraction.Ignore
        );

        if (hitSomething)
            return hit.point.y;

        Debug.LogWarning(
            "No stack surface found below spawn position. " +
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

        rb.linearDamping = 0.5f;
        rb.angularDamping = 2f;

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY;

        StackableItemPhysics stackable =
            stackedObject.GetComponent<StackableItemPhysics>();

        if (stackable == null)
            stackable = stackedObject.AddComponent<StackableItemPhysics>();

        Vector3 localPosition =
            carriage.transform.InverseTransformPoint(
                stackedObject.transform.position
            );

        stackable.Initialize(carriage, localPosition.z);
    }
}