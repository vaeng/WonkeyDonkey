using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ItemCollector : MonoBehaviour
{
    public event Action<CollectedItemInfo> OnItemCollected;

    [Header("References")]
    [SerializeField] private LaneMovement laneMovement;
    [SerializeField] private Carriage carriage;
    [SerializeField] private CarriageStackManager stackManager;

    [Header("Collector Size")]
    [SerializeField] private float collectorHeight = 1.5f;
    [SerializeField] private float collectorDepth = 1f;

    private BoxCollider collectorTrigger;

    private void Awake()
    {
        collectorTrigger = GetComponent<BoxCollider>();
        collectorTrigger.isTrigger = true;
    }

    private void Start()
    {
        UpdateCollectorSize();
    }

    private void UpdateCollectorSize()
    {
        if (carriage == null)
            return;

        collectorTrigger.size = new Vector3(
            carriage.GetPickupWidthInWorldUnits(),
            collectorHeight,
            collectorDepth
        );

        collectorTrigger.center = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        CollectableItem item = other.GetComponent<CollectableItem>();

        if (item == null || item.WasCollected)
            return;

        if (laneMovement == null || carriage == null)
            return;

        float itemLane = laneMovement.GetRoadLaneForVisualWorldX(other.transform.position.x);
        float carriageLane = laneMovement.CurrentCenterLane;

        bool foundSpot = carriage.TryGetClosestSpotForRoadLanePosition(
            itemLane,
            carriageLane,
            out int spotIndex
        );

        if (!foundSpot)
            return;

        CollectItem(item, spotIndex);
    }

    private void CollectItem(CollectableItem item, int spotIndex)
    {
        item.MarkAsCollected();

        Vector3 localSpot = carriage.GetLocalPositionForSpot(spotIndex);
        CollectedItemInfo itemInfo = new CollectedItemInfo(item, spotIndex, localSpot);

        if (stackManager != null)
            stackManager.PlaceCollectedItem(item, spotIndex);

        OnItemCollected?.Invoke(itemInfo);

        item.DestroyItem();
    }
}