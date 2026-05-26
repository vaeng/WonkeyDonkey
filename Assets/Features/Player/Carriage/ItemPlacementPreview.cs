using UnityEngine;

public class ItemPlacementPreview : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LaneMovement laneMovement;
    [SerializeField] private Carriage carriage;
    [SerializeField] private Transform stackRoot;
    [SerializeField] private GameObject previewLane;
    [SerializeField] private GameObject previewItemBox;

    [Header("Materials")]
    [SerializeField] private Material normalLaneMaterial;
    [SerializeField] private Material normalItemMaterial;
    [SerializeField] private Material fallLaneMaterial;
    [SerializeField] private Material fallItemMaterial;

    [Header("Detection")]
    [SerializeField] private LayerMask itemLayers;
    [SerializeField] private float previewDistance = 30f;
    [SerializeField] private LayerMask stackSurfaceLayers;
    [SerializeField] private int maxPlacementChecks = 8;
    [SerializeField] private float placementCheckPadding = 0.03f;

    private float previewDepth = 0.5f;
    private float raycastStartHeight = 100f;

    private void Awake()
    {
        SetPreviewVisible(false);
    }

    private void Update()
    {
        if (!CanShowPreview())
        {
            SetPreviewVisible(false);
            return;
        }

        CollectableItem item = GetNextItemInFront();

        if (item == null)
        {
            SetPreviewVisible(false);
            return;
        }

        float itemLane = laneMovement.GetRoadLaneForVisualWorldX(item.transform.position.x);

        bool hasSpot = carriage.TryGetClosestSpotForRoadLanePosition(
            itemLane,
            laneMovement.CurrentCenterLane,
            out int spotIndex
        );

        if (!hasSpot)
        {
            SetPreviewVisible(false);
            return;
        }

        bool itemWillFall = !carriage.IsSpotOnCarriage(spotIndex);

        SetPreviewMaterial(itemWillFall);

        if (itemWillFall)
            ShowFallPreviewFor(item, spotIndex);
        else
            ShowPreviewFor(item, spotIndex);
    }

    private bool CanShowPreview()
    {
        return laneMovement != null
            && carriage != null
            && stackRoot != null
            && previewLane != null
            && previewItemBox != null;
    }

    private CollectableItem GetNextItemInFront()
    {
        CollectableItem closestItem = null;
        float closestDistance = float.MaxValue;

        Vector3 boxCenter = transform.position + Vector3.forward * (previewDistance * 0.5f);

        Vector3 boxHalfSize = new Vector3(
            carriage.GetPickupWidthInWorldUnits() * 0.5f,
            0.5f,
            previewDistance * 0.5f
        );

        Collider[] hits = Physics.OverlapBox(
            boxCenter,
            boxHalfSize,
            Quaternion.identity,
            itemLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            CollectableItem item = hit.GetComponentInParent<CollectableItem>();

            if (item == null || item.WasCollected)
                continue;

            float distance = item.transform.position.z - transform.position.z;

            if (distance <= 0f || distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestItem = item;
        }

        return closestItem;
    }

    private void ShowPreviewFor(CollectableItem item, int spotIndex)
    {
        Vector3 localSpot = carriage.GetLocalPositionForSpot(spotIndex);
        Vector3 spotPosition = stackRoot.TransformPoint(localSpot);

        float itemWidth = item.WidthInLanes * carriage.GetLaneWidth();
        float itemHeight = item.HeightInLanes;

        float itemBottomY = FindFreeBottomY(spotPosition, itemWidth, itemHeight);
        float columnHeight = Mathf.Max(0.01f, itemBottomY - stackRoot.position.y);

        SetBox(
            previewLane,
            new Vector3(spotPosition.x, stackRoot.position.y + columnHeight * 0.5f, spotPosition.z),
            new Vector3(itemWidth, columnHeight, previewDepth)
        );

        SetBox(
            previewItemBox,
            new Vector3(spotPosition.x, itemBottomY + itemHeight * 0.5f, spotPosition.z),
            new Vector3(itemWidth, itemHeight, previewDepth)
        );

        SetPreviewVisible(true);
    }

    private void ShowFallPreviewFor(CollectableItem item, int spotIndex)
    {
        Vector3 localSpot = carriage.GetLocalPositionForSpot(spotIndex);
        Vector3 spotPosition = stackRoot.TransformPoint(localSpot);

        float itemWidth = item.WidthInLanes * carriage.GetLaneWidth();
        float itemHeight = item.HeightInLanes;

        float columnHeight = 0.7f;
        float bottomY = stackRoot.position.y;

        SetBox(
            previewLane,
            new Vector3(spotPosition.x, stackRoot.position.y + columnHeight * 0.5f, spotPosition.z),
            new Vector3(itemWidth, columnHeight, previewDepth)
        );

        SetBox(
            previewItemBox,
            new Vector3(spotPosition.x, bottomY + itemHeight * 0.5f, spotPosition.z),
            new Vector3(itemWidth, itemHeight, previewDepth)
        );

        SetPreviewVisible(true);
    }

    private float FindFreeBottomY(Vector3 center, float itemWidth, float itemHeight)
    {
        float bottomY = GetSurfaceYAt(center, itemWidth);

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
                previewDepth * 0.5f - placementCheckPadding
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

            bottomY = blockedUntilY + 0.03f;
        }

        return bottomY;
    }

    private float GetSurfaceYAt(Vector3 center, float itemWidth)
    {
        float highestY = stackRoot.position.y;
        float offset = itemWidth * 0.225f;

        CheckSurface(center - carriage.transform.right * offset, ref highestY);
        CheckSurface(center, ref highestY);
        CheckSurface(center + carriage.transform.right * offset, ref highestY);

        return highestY;
    }

    private void CheckSurface(Vector3 position, ref float highestY)
    {
        Vector3 rayStart = position + Vector3.up * raycastStartHeight;

        bool hitSomething = Physics.Raycast(
            rayStart,
            Vector3.down,
            out RaycastHit hit,
            raycastStartHeight,
            stackSurfaceLayers,
            QueryTriggerInteraction.Ignore
        );

        if (hitSomething && hit.point.y > highestY)
            highestY = hit.point.y;
    }

    private void SetBox(GameObject box, Vector3 position, Vector3 scale)
    {
        box.transform.position = position;
        box.transform.rotation = stackRoot.rotation;
        box.transform.localScale = scale;
    }

    private void SetPreviewMaterial(bool itemWillFall)
    {
        if (itemWillFall)
        {
            SetMaterial(previewLane, fallLaneMaterial);
            SetMaterial(previewItemBox, fallItemMaterial);
        }
        else
        {
            SetMaterial(previewLane, normalLaneMaterial);
            SetMaterial(previewItemBox, normalItemMaterial);
        }
    }

    private void SetMaterial(GameObject obj, Material material)
    {
        if (obj == null || material == null)
            return;

        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();

        if (renderer != null)
            renderer.material = material;
    }

    private void SetPreviewVisible(bool visible)
    {
        if (previewLane != null)
            previewLane.SetActive(visible);

        if (previewItemBox != null)
            previewItemBox.SetActive(visible);
    }

    private void OnDrawGizmosSelected()
    {
        if (carriage == null)
            return;

        Gizmos.color = Color.cyan;

        Vector3 boxCenter = transform.position + Vector3.forward * (previewDistance * 0.5f);

        Vector3 boxSize = new Vector3(
            carriage.GetPickupWidthInWorldUnits(),
            1f,
            previewDistance
        );

        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}