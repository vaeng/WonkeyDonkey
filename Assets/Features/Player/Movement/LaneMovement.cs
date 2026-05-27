using UnityEngine;

public class LaneMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SwipeInput swipeInput;
    [SerializeField] private Carriage carriage;
    [SerializeField] private LaneSystem laneSystem;
    [SerializeField] private AnimationSystem animationSystem;

    [Header("Lane Movement")]
    [SerializeField] private float laneStepPerSwipe = 0.5f;
    [SerializeField] private float sideMoveSpeed = 10f;

    [Header("Start")]
    [SerializeField] private float startCenterLane = 3f;

    private float currentCenterLane;
    private float currentWorldOffsetX;
    private float targetWorldOffsetX;

    private float minCenterLane;
    private float maxCenterLane;

    public float CurrentCenterLane => currentCenterLane;

    private void Awake()
    {
        if (!SetupLaneRange())
            return;

        currentCenterLane = Mathf.Clamp(startCenterLane, minCenterLane, maxCenterLane);
        currentWorldOffsetX = GetWorldOffsetForLane(currentCenterLane);
        targetWorldOffsetX = currentWorldOffsetX;

        KeepPlayerAtWorldCenter();
    }

    private void OnEnable()
    {
        if (swipeInput == null)
            return;

        swipeInput.OnSwipeLeft += MoveLeft;
        swipeInput.OnSwipeRight += MoveRight;
    }

    private void OnDisable()
    {
        if (swipeInput == null)
            return;

        swipeInput.OnSwipeLeft -= MoveLeft;
        swipeInput.OnSwipeRight -= MoveRight;
    }

    private void FixedUpdate()
    {
        currentWorldOffsetX = Mathf.MoveTowards(
            currentWorldOffsetX,
            targetWorldOffsetX,
            sideMoveSpeed * Time.fixedDeltaTime
        );

        KeepPlayerAtWorldCenter();
    }

    private void MoveLeft()
    {
        SetTargetLane(currentCenterLane - laneStepPerSwipe);
        animationSystem?.StartAnimation(1f);
    }

    private void MoveRight()
    {
        SetTargetLane(currentCenterLane + laneStepPerSwipe);
        animationSystem?.StartAnimation(-1f);

        //fmod spielt einen oneshot aus der Bank Donkey ab
        FMODUnity.RuntimeManager.PlayOneShot("event:/Donkey");
        FMODUnity.RuntimeManager.PlayOneShot("event:/Knarz");
    }

    private void SetTargetLane(float lane)
    {
        currentCenterLane = Mathf.Clamp(lane, minCenterLane, maxCenterLane);
        targetWorldOffsetX = GetWorldOffsetForLane(currentCenterLane);
    }

    private bool SetupLaneRange()
    {
        if (carriage == null || laneSystem == null)
        {
            enabled = false;
            return false;
        }

        int carriageLaneCount = carriage.GetCarriageLaneCount();

        if (carriageLaneCount > laneSystem.RoadLaneCount)
        {
            enabled = false;
            return false;
        }

        float sideExtension = (carriageLaneCount - 1f) * 0.5f;

        minCenterLane = sideExtension;
        maxCenterLane = laneSystem.MaxRoadLane - sideExtension;

        return true;
    }

    private float GetWorldOffsetForLane(float lane)
    {
        return -laneSystem.GetWorldXForLanePosition(lane);
    }

    private void KeepPlayerAtWorldCenter()
    {
        Vector3 position = transform.position;
        position.x = 0f;
        transform.position = position;
    }

    public float GetVisualWorldXForRoadX(float roadX)
    {
        return roadX + currentWorldOffsetX;
    }

    public float GetRoadLaneForVisualWorldX(float visualWorldX)
    {
        float roadX = visualWorldX - currentWorldOffsetX;
        return laneSystem.GetLanePositionForWorldX(roadX);
    }
}