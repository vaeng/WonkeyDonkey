using UnityEngine;

/// <summary>
/// Handles lane-based player movement. The player moves forward automatically and can switch lanes based on swipe input.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private SwipeInput swipeInput;

    [Header("Lane Settings")]
    [SerializeField] private int roadLaneCount = 7;
    [SerializeField] private float laneWidth = 1.5f;
    [SerializeField] private float laneStepPerSwipe = 0.5f;

    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 6f;
    [SerializeField] private float sideMoveSpeed = 10f;

    [Header("Start")]
    [SerializeField] private float startCenterLane = 3f;

    private float currentCenterLane;
    private float minCenterLane;
    private float maxCenterLane;

    private void Awake()
    {
        minCenterLane = 1f;
        maxCenterLane = roadLaneCount - 2f;

        currentCenterLane = Mathf.Clamp(startCenterLane, minCenterLane, maxCenterLane);
    }

    private void OnEnable()
    {
        swipeInput.OnSwipeLeft += MoveLeft;
        swipeInput.OnSwipeRight += MoveRight;
    }

    private void OnDisable()
    {
        swipeInput.OnSwipeLeft -= MoveLeft;
        swipeInput.OnSwipeRight -= MoveRight;
    }

    private void Update()
    {
        MoveForward();
        MoveSidewaysToCurrentLane();
    }

    private void MoveForward()
    {
        transform.position += Vector3.forward * forwardSpeed * Time.deltaTime;
    }

    private void MoveSidewaysToCurrentLane()
    {
        Vector3 targetPosition = transform.position;
        targetPosition.x = GetWorldXForLanePosition(currentCenterLane);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            sideMoveSpeed * Time.deltaTime
        );
    }

    private void MoveLeft()
    {
        currentCenterLane = Mathf.Clamp(
            currentCenterLane - laneStepPerSwipe,
            minCenterLane,
            maxCenterLane
        );

        Debug.Log("Move Left. Current Center Lane: " + currentCenterLane);
    }

    private void MoveRight()
    {
        currentCenterLane = Mathf.Clamp(
            currentCenterLane + laneStepPerSwipe,
            minCenterLane,
            maxCenterLane
        );

        Debug.Log("Move Right. Current Center Lane: " + currentCenterLane);
    }

    public float GetCurrentCenterLane()
    {
        return currentCenterLane;
    }

    public float GetLaneWidth()
    {
        return laneWidth;
    }

    public float GetWorldXForLanePosition(float lanePosition)
    {
        float roadCenterOffset = (roadLaneCount - 1) * 0.5f;
        return (lanePosition - roadCenterOffset) * laneWidth;
    }
}
