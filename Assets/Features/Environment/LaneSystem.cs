using UnityEngine;

public class LaneSystem : MonoBehaviour
{
    [Header("Road Lanes")]
    [SerializeField] private int roadLaneCount = 7;
    [SerializeField] private float laneWidth = 1f;

    public int RoadLaneCount => roadLaneCount;
    public float LaneWidth => laneWidth;

    public float MinRoadLane => 0f;
    public float MaxRoadLane => roadLaneCount - 1f;

    private float RoadCenterOffset => (roadLaneCount - 1) * 0.5f;

    public float GetWorldXForLanePosition(float lanePosition)
    {
        return (lanePosition - RoadCenterOffset) * laneWidth;
    }

    public float GetLanePositionForWorldX(float worldX)
    {
        return worldX / laneWidth + RoadCenterOffset;
    }

    public int GetNearestRoadLaneForWorldX(float worldX)
    {
        return Mathf.RoundToInt(GetLanePositionForWorldX(worldX));
    }

    public bool IsValidRoadLanePosition(float lanePosition)
    {
        return lanePosition >= MinRoadLane && lanePosition <= MaxRoadLane;
    }
}