using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ItemCollector : MonoBehaviour
{
    public event Action<CollectedItemInfo> OnItemCollected;

    /// <summary>Used for lane position calculations.</summary>
    [Header("References")]
    [SerializeField] private LaneSystem laneSystem;
    /// <summary>Used to determine the player's current center lane position.</summary>
    [SerializeField] private PlayerMovement playerMovement;
    /// <summary>Used to determine the width of the carriage and available spots for collected items.</summary>
    [SerializeField] private Carriage carriage;

    /// <summary>Height of the collector trigger area in world units.</summary>
    [Header("Collector Size")]
    [SerializeField] private float collectorHeight = 1.5f;
    /// <summary>Depth of the collector trigger area in world units.</summary>
    [SerializeField] private float collectorDepth = 1f;

    private BoxCollider boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
    }

    private void Start()
    {
        UpdateCollectorSize();
    }

    private void UpdateCollectorSize()
    {
        boxCollider.size = new Vector3(
            carriage.GetCarriageWidthInWorldUnits(),
            collectorHeight,
            collectorDepth
        );

        boxCollider.center = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        CollectableItem item = other.GetComponent<CollectableItem>();

        if (item == null)
            return;

        if (item.WasCollected)
            return;

        float itemWorldX = other.transform.position.x;
        float roadLanePosition = laneSystem.GetLanePositionForWorldX(itemWorldX);
        float carriageCenterLane = playerMovement.GetCurrentCenterLane();

        bool hasValidSpot = carriage.TryGetClosestSpotForWorldX(
            itemWorldX,
            carriageCenterLane,
            out int carriageSpotIndex
        );

        if (!hasValidSpot)
        {
            Debug.LogWarning(
                "Item touched collector but was outside carriage spots: " +
                other.gameObject.name
            );

            return;
        }

        Vector3 carriageLocalSpotPosition =
            carriage.GetLocalPositionForSpot(carriageSpotIndex);

        CollectedItemInfo collectedItemInfo = new CollectedItemInfo(
            item,
            carriageSpotIndex,
            carriageLocalSpotPosition
        );

        Debug.Log(collectedItemInfo.Item + " collected on carriage spot: " + collectedItemInfo.CarriageSpotIndex);

        item.MarkAsCollected();
        item.DestroyItem();

        OnItemCollected?.Invoke(collectedItemInfo);
    }
}