using UnityEngine;

public class CollectedItemInfo
{
    public CollectableItem Item { get; }
    public int CarriageSpotIndex { get; }
    public Vector3 CarriageLocalSpotPosition { get; }

    public CollectedItemInfo(
        CollectableItem item,
        int carriageSpotIndex,
        Vector3 carriageLocalSpotPosition
    )
    {
        Item = item;
        CarriageSpotIndex = carriageSpotIndex;
        CarriageLocalSpotPosition = carriageLocalSpotPosition;
    }
}