using UnityEngine;
using FMODUnity;

public class LaneMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SwipeInput swipeInput;
    [SerializeField] private Carriage carriage;
    [SerializeField] private LaneSystem laneSystem;
    [SerializeField] private AnimationSystem animationSystem;
    [SerializeField] private EventReference donkeyScreamSoundEvent, creakingSoundEvent;
    [SerializeField] private EventReference donkeyHoovesSoundEvent;
    private FMOD.Studio.EventInstance donkeyHoovesInstance;
    private bool isHoovesSoundPlaying = false;

    [SerializeField] private float donkeyScreamCoolDown = 0.2f, creakingCoolDown;
    private float donkeyScreamLastSoundTime, creakingLastSoundTime;

    [Header("Lane Movement")]
    [SerializeField] private float laneStepPerSwipe = 0.5f;
    [SerializeField] private float sideMoveSpeed = 5f;

    [Header("Start")]
    [SerializeField] private float startCenterLane = 3f;

    private float currentCenterLane;
    private float currentWorldOffsetX;
    private float targetWorldOffsetX;

    private float minCenterLane;
    private float maxCenterLane;

    private int heldDirection;

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
        swipeInput.OnSwipeReleased += StopLaneMovement;
    }

    private void OnDisable()
    {
        if (swipeInput == null)
            return;

        swipeInput.OnSwipeLeft -= MoveLeft;
        swipeInput.OnSwipeRight -= MoveRight;
        swipeInput.OnSwipeReleased -= StopLaneMovement;
    }

    private void FixedUpdate()
    {
        currentWorldOffsetX = Mathf.MoveTowards(
            currentWorldOffsetX,
            targetWorldOffsetX,
            sideMoveSpeed * Time.fixedDeltaTime
        );

        if (heldDirection != 0 && HasReachedTargetLane())
            MoveInHeldDirection();

        KeepPlayerAtWorldCenter();
    }

    // Sound Beim Start abspielen
    public void StartRun()
    {
        donkeyHoovesInstance = FMODUnity.RuntimeManager.CreateInstance(donkeyHoovesSoundEvent);
        donkeyHoovesInstance.start();
        isHoovesSoundPlaying = true;
    }

    // Sound beenden Wenn Spieler bei Finish angekommen ist
    public void OnFinish()
    {
        if (isHoovesSoundPlaying)
        {
            donkeyHoovesInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            donkeyHoovesInstance.release();
            isHoovesSoundPlaying = false;
        }
    }
    private void MoveLeft()
    {
        heldDirection = -1;
        SetTargetLane(currentCenterLane - laneStepPerSwipe);
        animationSystem?.StartAnimation(1f);

        PlaySoundEffectWhenMoving();
    }

    private void MoveRight()
    {
        heldDirection = 1;
        SetTargetLane(currentCenterLane + laneStepPerSwipe);
        animationSystem?.StartAnimation(-1f);

        PlaySoundEffectWhenMoving();
    }

    private void MoveInHeldDirection()
    {
        if (heldDirection < 0)
            MoveLeft();
        else if (heldDirection > 0)
            MoveRight();
    }

    private void PlaySoundEffectWhenMoving()
    {
        float donkeyScreamtimeSinceLastSound = Time.time - donkeyScreamLastSoundTime;

        if (donkeyScreamtimeSinceLastSound >= donkeyScreamCoolDown)
        {
            RuntimeManager.PlayOneShot(donkeyScreamSoundEvent, transform.position);
            donkeyScreamLastSoundTime = Time.time;
        }

        float creakingTimeSinceLastSound = Time.time - creakingLastSoundTime;

        if (creakingTimeSinceLastSound >= creakingCoolDown)
        {
            RuntimeManager.PlayOneShot(creakingSoundEvent, transform.position);
            creakingLastSoundTime = Time.time;
        }
    }

    private void StopLaneMovement()
    {
        heldDirection = 0;
    }

    private bool HasReachedTargetLane()
    {
        return Mathf.Approximately(currentWorldOffsetX, targetWorldOffsetX);
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