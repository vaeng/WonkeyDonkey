using UnityEngine;

public class WorldMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    private LaneMovement laneMovement;
    private float roadX;
    private bool hasLaneMovement;

    public void Initialize(LaneMovement laneMovement, float moveSpeed, float roadX)
    {
        this.laneMovement = laneMovement;
        this.moveSpeed = moveSpeed;
        this.roadX = roadX;

        hasLaneMovement = laneMovement != null;

        UpdateVisualX();
    }

    private void FixedUpdate()
    {
        Vector3 position = transform.position;
        position.z -= moveSpeed * Time.fixedDeltaTime;
        transform.position = position;

        UpdateVisualX();
    }

    public void SetMoveSpeed(float newMoveSpeed)
    {
        moveSpeed = newMoveSpeed;
    }

    private void UpdateVisualX()
    {
        if (!hasLaneMovement)
            return;

        Vector3 position = transform.position;
        position.x = laneMovement.GetVisualWorldXForRoadX(roadX);
        transform.position = position;
    }
}