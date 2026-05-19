using UnityEngine;

public class CarriageStackManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Carriage carriage;
    [SerializeField] private Transform stackRoot;

    [Header("Spawn")]
    [SerializeField] private float spawnBottomOffset = 0.05f;
    [SerializeField] private float raycastStartHeight = 10f;
    [SerializeField] private float raycastDistance = 20f;
    [SerializeField] private float itemDepth = 0.8f;
    [SerializeField] private bool scaleStackedItemsFromData = false;

    [Header("Physics")]
    [SerializeField] private float linearDamping = 0.2f;
    [SerializeField] private float angularDamping = 0.8f;

    [Header("Raycast")]
    [SerializeField] private LayerMask stackSurfaceLayers = ~0;

    public void PlaceCollectedItem(CollectableItem item, int spotIndex)
    {
        if (!CanPlaceItem(item))
            return;

        Vector3 localSpot = carriage.GetLocalPositionForSpot(spotIndex);
        Vector3 spawnBase = stackRoot.TransformPoint(new Vector3(localSpot.x, 0f, 0f));

        GameObject stackedItem = Instantiate(item.StackedPrefab, spawnBase, stackRoot.rotation);

        RemoveWorldMover(stackedItem);
        ScaleItemIfNeeded(stackedItem, item);
        MoveItemOnTopOfStack(stackedItem, spawnBase, item);
        SetupPhysics(stackedItem, item);

        Debug.Log($"Placed {item.ItemType} on carriage spot {spotIndex}.");
    }

    private bool CanPlaceItem(CollectableItem item)
    {
        if (carriage == null || stackRoot == null)
        {
            Debug.LogWarning("CarriageStackManager: Missing carriage or stack root.");
            return false;
        }

        if (item == null || item.StackedPrefab == null)
        {
            Debug.LogWarning("CarriageStackManager: Missing collected item or stacked prefab.");
            return false;
        }

        return true;
    }

    private void RemoveWorldMover(GameObject stackedItem)
    {
        WorldMover mover = stackedItem.GetComponent<WorldMover>();

        if (mover != null)
            Destroy(mover);
    }

    private void ScaleItemIfNeeded(GameObject stackedItem, CollectableItem item)
    {
        if (!scaleStackedItemsFromData)
            return;

        stackedItem.transform.localScale = new Vector3(
            item.WidthInLanes,
            item.HeightInLanes,
            itemDepth
        );
    }

    private void MoveItemOnTopOfStack(GameObject stackedItem, Vector3 spawnBase, CollectableItem item)
    {
        Physics.SyncTransforms();

        Collider itemCollider = stackedItem.GetComponentInChildren<Collider>();

        if (itemCollider == null)
        {
            Debug.LogWarning($"{stackedItem.name} has no collider.");
            return;
        }

        // Collider kurz anlassen, damit bounds korrekt sind
        Bounds itemBounds = itemCollider.bounds;
        Vector3 localBottomOffset = itemCollider.bounds.min - stackedItem.transform.position;

        itemCollider.enabled = false;
        Physics.SyncTransforms();

        float itemWidth = item.WidthInLanes * carriage.GetLaneWidth();
        float surfaceY = GetHighestSurfaceY(spawnBase, itemWidth);

        Vector3 finalPosition = spawnBase;
        finalPosition.y = surfaceY + spawnBottomOffset - localBottomOffset.y;

        stackedItem.transform.position = finalPosition;

        itemCollider.enabled = true;
        Physics.SyncTransforms();
    }

    private float GetHighestSurfaceY(Vector3 center, float itemWidth)
    {
        float highestY = float.MinValue;
        float halfWidth = itemWidth * 0.5f;

        Vector3[] rayOffsets =
        {
            Vector3.left * halfWidth * 0.45f,
            Vector3.zero,
            Vector3.right * halfWidth * 0.45f
        };

        foreach (Vector3 offset in rayOffsets)
        {
            Vector3 samplePosition = center + carriage.transform.TransformDirection(offset);
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

            highestY = Mathf.Max(highestY, hit.point.y);
        }

        if (highestY != float.MinValue)
            return highestY;

        Debug.LogWarning("CarriageStackManager: No stack surface found. Using stack root height.");

        return stackRoot.position.y;
    }

    private void SetupPhysics(GameObject stackedItem, CollectableItem item)
    {
        Rigidbody rb = stackedItem.GetComponent<Rigidbody>();

        if (rb == null)
            rb = stackedItem.AddComponent<Rigidbody>();

        rb.mass = item.Mass;
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

        StackableItemPhysics itemPhysics = stackedItem.GetComponent<StackableItemPhysics>();

        if (itemPhysics == null)
            itemPhysics = stackedItem.AddComponent<StackableItemPhysics>();

        itemPhysics.Initialize(carriage);
    }
}