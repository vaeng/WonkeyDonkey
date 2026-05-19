using UnityEngine;

public class Carriage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LaneSystem laneSystem;

    [Header("Carriage")]
    [SerializeField] private int carriageLaneCount = 3;

    public int GetCarriageLaneCount()
    {
        return carriageLaneCount;
    }

    public int GetCarriageSpotCount()
    {
        return carriageLaneCount * 2 - 1;
    }

    public float GetCarriageWidthInWorldUnits()
    {
        return carriageLaneCount * laneSystem.LaneWidth;
    }

    public float GetLaneWidth()
    {
        return laneSystem.LaneWidth;
    }

    public bool TryGetClosestSpotForWorldX(float worldX, float carriageCenterLane, out int spotIndex)
    {
        float itemLane = laneSystem.GetLanePositionForWorldX(worldX);
        return TryGetClosestSpotForRoadLanePosition(itemLane, carriageCenterLane, out spotIndex);
    }

    public bool TryGetClosestSpotForRoadLanePosition(float itemLane, float carriageCenterLane, out int spotIndex)
    {
        float leftLane = carriageCenterLane - (carriageLaneCount - 1) * 0.5f;
        float laneOnCarriage = itemLane - leftLane;

        spotIndex = Mathf.RoundToInt(laneOnCarriage * 2f);

        return spotIndex >= 0 && spotIndex < GetCarriageSpotCount();
    }

    public Vector3 GetLocalPositionForSpot(int spotIndex)
    {
        spotIndex = Mathf.Clamp(spotIndex, 0, GetCarriageSpotCount() - 1);

        float leftX = -(carriageLaneCount - 1) * 0.5f * laneSystem.LaneWidth;
        float x = leftX + spotIndex * 0.5f * laneSystem.LaneWidth;

        return new Vector3(x, 0f, 0f);
    }
}