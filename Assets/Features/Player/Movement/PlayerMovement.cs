using UnityEngine;

/// <summary>
/// Moves the player sideways between lane positions.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
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
    [SerializeField] private float laneStepPerSwipe = 0.5f;

    /// <summary>Sideways movement speed in units per second.</summary>
    [SerializeField] private float sideMoveSpeed = 10f;

    /// <summary>Initial center lane position at the start of the game.</summary>
    [Header("Start")]
    [SerializeField] private float startCenterLane = 3f;

    private Rigidbody rb;

    private float currentCenterLane;
    private float minCenterLane;
    private float maxCenterLane;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        CalculateAllowedLaneRange();

        currentCenterLane = Mathf.Clamp(
            startCenterLane,
            minCenterLane,
            maxCenterLane
        );

        Vector3 startPosition = rb.position;

        startPosition.x =
            laneSystem.GetWorldXForLanePosition(currentCenterLane);

        rb.position = startPosition;
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

    private void FixedUpdate()
    {
        MoveSidewaysWithRigidbody();
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

        float carriageSideExtensionInLanes =
            (carriageLaneCount - 1f) / 2f;

        minCenterLane = carriageSideExtensionInLanes;

        maxCenterLane =
            laneSystem.MaxRoadLane - carriageSideExtensionInLanes;
    }

    private void MoveSidewaysWithRigidbody()
    {
        Vector3 currentPosition = rb.position;

        float targetX =
            laneSystem.GetWorldXForLanePosition(currentCenterLane);

        float newX = Mathf.MoveTowards(
            currentPosition.x,
            targetX,
            sideMoveSpeed * Time.fixedDeltaTime
        );

        Vector3 nextPosition = new Vector3(
            newX,
            currentPosition.y,
            currentPosition.z
        );

        rb.MovePosition(nextPosition);
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