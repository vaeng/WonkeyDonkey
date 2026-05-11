using UnityEngine;

/// <summary>
/// Moves the player forward automatically and sideways between lane positions.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    /// <summary>Detects player swipes.</summary>
    [Header("References")]
    [SerializeField] private SwipeInput swipeInput;
    /// <summary>Defines the player's width in lanes.</summary>
    [SerializeField] private Carriage carriage;
    /// <summary>Used for lane position calculations.</summary>
    [SerializeField] private LaneSystem laneSystem;

    /// <summary>How many lane positions the player moves per swipe.</summary>
    [Header("Lane Movement")]
    [SerializeField] private float laneStepPerSwipe = 0.5f; // 

    /// <summary>Forward movement speed in units per second.</summary>
    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 6f;
    /// <summary>Sideways movement speed in units per second.</summary>
    [SerializeField] private float sideMoveSpeed = 10f;

    /// <summary>Initial center lane position at the start of the game.</summary>
    [Header("Start")]
    [SerializeField] private float startCenterLane = 3f; // Default starting lane position

    private float currentCenterLane;
    /// <summary>Leftmost allowed center lane position</summary>
    private float minCenterLane;
    /// <summary>Rightmost allowed center lane position</summary>
    private float maxCenterLane;

    private void Awake()
    {
        CalculateAllowedLaneRange();
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

    private void CalculateAllowedLaneRange()
    {
        int carriageLaneCount = carriage.GetCarriageLaneCount();

        if (carriageLaneCount > laneSystem.RoadLaneCount)
        {
            Debug.LogError(
                "PlayerMovement setup is invalid: Carriage is wider than the road. " +
                "Carriage Lane Count: " + carriageLaneCount +
                " | Road Lane Count: " + laneSystem.RoadLaneCount
            );

            enabled = false;
            return;
        }

        float carriageSideExtensionInLanes = (carriageLaneCount - 1f) / 2f;

        minCenterLane = carriageSideExtensionInLanes;
        maxCenterLane = laneSystem.MaxRoadLane - carriageSideExtensionInLanes;
    }

    private void MoveForward()
    {
        transform.position += Vector3.forward * forwardSpeed * Time.deltaTime;
    }

    private void MoveSidewaysToCurrentLane()
    {
        Vector3 targetPosition = transform.position;
        targetPosition.x = laneSystem.GetWorldXForLanePosition(currentCenterLane);

        transform.position = Vector3.MoveTowards(
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
    }

    private void MoveRight()
    {
        currentCenterLane = Mathf.Clamp(
            currentCenterLane + laneStepPerSwipe,
            minCenterLane,
            maxCenterLane
        );
    }

    public float GetCurrentCenterLane()
    {
        return currentCenterLane;
    }
}