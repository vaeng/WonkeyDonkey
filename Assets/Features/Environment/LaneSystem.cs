using UnityEngine;

public class LaneSystem : MonoBehaviour
{
    /// <summary>Total number of lanes on the road.</summary>
    [Header("Road Lane Settings")]
    [SerializeField] private int roadLaneCount = 7;
    /// <summary>Width of each lane in world units.</summary>
    [SerializeField] private float laneWidth = 1f;

    /// <summary>Number of lanes the player occupies based on the carriage settings.</summary>
    public int RoadLaneCount => roadLaneCount;
    /// <summary>Width of each lane in world units.</summary>
    public float LaneWidth => laneWidth;

    /// <summary>Minimum valid lane position (leftmost lane).</summary>
    public float MinRoadLane => 0f;
    /// <summary>Maximum valid lane position (rightmost lane).</summary>
    public float MaxRoadLane => roadLaneCount - 1f;

    public float GetWorldXForLanePosition(float lanePosition)
    {
        float roadCenterOffset = (roadLaneCount - 1) * 0.5f;
        return (lanePosition - roadCenterOffset) * laneWidth;
    }

    public float GetLanePositionForWorldX(float worldX)
    {
        float roadCenterOffset = (roadLaneCount - 1) * 0.5f;
        return (worldX / laneWidth) + roadCenterOffset;
    }

    public int GetNearestRoadLaneForWorldX(float worldX)
    {
        float lanePosition = GetLanePositionForWorldX(worldX);
        return Mathf.RoundToInt(lanePosition);
    }

    public bool IsValidRoadLanePosition(float lanePosition)
    {
        return lanePosition >= MinRoadLane && lanePosition <= MaxRoadLane;
    }
}