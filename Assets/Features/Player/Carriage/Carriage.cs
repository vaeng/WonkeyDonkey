using UnityEngine;

public class Carriage : MonoBehaviour
{
    /// <summary>Used for lane position calculations.</summary>
    [Header("References")]
    [SerializeField] private LaneSystem laneSystem;

    /// <summary>Number of lanes the carriage occupies.</summary>
    [Header("Carriage Settings")]
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

    public bool TryGetClosestSpotForWorldX(
        float worldX,
        float carriageCenterLane,
        out int spotIndex
    )
    {
        float itemRoadLanePosition =
            laneSystem.GetLanePositionForWorldX(worldX);

        float leftCarriageLaneCenter =
            carriageCenterLane - ((carriageLaneCount - 1) * 0.5f);

        float relativeLanePosition =
            itemRoadLanePosition - leftCarriageLaneCenter;

        int calculatedSpotIndex =
            Mathf.RoundToInt(relativeLanePosition * 2f);

        if (calculatedSpotIndex < 0 || calculatedSpotIndex >= GetCarriageSpotCount())
        {
            spotIndex = -1;
            return false;
        }

        spotIndex = calculatedSpotIndex;
        return true;
    }

    public Vector3 GetLocalPositionForSpot(int spotIndex)
    {
        spotIndex = Mathf.Clamp(
            spotIndex,
            0,
            GetCarriageSpotCount() - 1
        );

        float laneWidth = laneSystem.LaneWidth;

        float leftX =
            -((carriageLaneCount - 1) * 0.5f * laneWidth);

        float x =
            leftX + (spotIndex * 0.5f * laneWidth);

        return new Vector3(x, 0f, 0f);
    }

    public float GetLaneWidth()
    {
        return laneSystem.LaneWidth;
    }
}