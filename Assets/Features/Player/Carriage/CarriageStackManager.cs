using UnityEngine;

public class CarriageStackManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Carriage carriage;
    [SerializeField] private Transform stackRoot;

    [Header("Spawn")]
    [SerializeField] private float spawnBottomOffset = 0.03f;
    [SerializeField] private float raycastStartHeight = 10f;
    [SerializeField] private float raycastDistance = 20f;
    [SerializeField] private float itemDepth = 0.8f;
    [SerializeField] private bool scaleStackedItemsFromData = false;
    [SerializeField] private int maxPlacementChecks = 8;
    [SerializeField] private float placementCheckPadding = 0.03f;

    [Header("Physics")]
    [SerializeField] private float linearDamping = 0.5f;
    [SerializeField] private float angularDamping = 1.5f;

    [Header("Layers")]
    [SerializeField] private string stackItemLayerName = "StackItem";

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
        SetLayerRecursively(stackedItem, LayerMask.NameToLayer(stackItemLayerName));
        ScaleItemIfNeeded(stackedItem, item);
        MoveItemOnTopOfStack(stackedItem, spawnBase, item);
        SetupPhysics(stackedItem, item);
    }

    private bool CanPlaceItem(CollectableItem item)
    {
        if (carriage == null || stackRoot == null)
        {
            return false;
        }

        if (item == null || item.StackedPrefab == null)
        {
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

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (layer < 0)
        {
            return;
        }

        obj.layer = layer;

        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
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
            return;
        }

        Vector3 localBottomOffset = itemCollider.bounds.min - stackedItem.transform.position;

        itemCollider.enabled = false;
        Physics.SyncTransforms();

        float itemWidth = item.WidthInLanes * carriage.GetLaneWidth();
        float itemHeight = item.HeightInLanes;

        float surfaceY = GetHighestSurfaceY(spawnBase, itemWidth);
        float bottomY = FindFreeBottomY(spawnBase, itemWidth, itemHeight, surfaceY);

        Vector3 finalPosition = spawnBase;
        finalPosition.y = bottomY + spawnBottomOffset - localBottomOffset.y;

        stackedItem.transform.position = finalPosition;

        itemCollider.enabled = true;
        Physics.SyncTransforms();
    }

    private float FindFreeBottomY(Vector3 center, float itemWidth, float itemHeight, float startY)
    {
        float bottomY = startY + spawnBottomOffset;

        for (int i = 0; i < maxPlacementChecks; i++)
        {
            Vector3 boxCenter = new Vector3(
                center.x,
                bottomY + itemHeight * 0.5f,
                center.z
            );

            Vector3 halfSize = new Vector3(
                itemWidth * 0.5f - placementCheckPadding,
                itemHeight * 0.5f - placementCheckPadding,
                itemDepth * 0.5f - placementCheckPadding
            );

            Collider[] hits = Physics.OverlapBox(
                boxCenter,
                halfSize,
                stackRoot.rotation,
                stackSurfaceLayers,
                QueryTriggerInteraction.Ignore
            );

            float blockedUntilY = bottomY;
            bool isBlocked = false;

            foreach (Collider hit in hits)
            {
                if (hit.transform.IsChildOf(carriage.transform))
                    continue;

                isBlocked = true;
                blockedUntilY = Mathf.Max(blockedUntilY, hit.bounds.max.y);
            }

            if (!isBlocked)
                return bottomY;

            bottomY = blockedUntilY + spawnBottomOffset;
        }

        return bottomY;
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

        StackableItemPhysics itemPhysics = stackedItem.GetComponent<StackableItemPhysics>();

        if (itemPhysics == null)
            itemPhysics = stackedItem.AddComponent<StackableItemPhysics>();

        itemPhysics.Initialize(carriage);
    }
}