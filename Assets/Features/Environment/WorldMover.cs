using UnityEngine;

/// <summary>
/// Moves world objects backwards to simulate player forward movement.
/// </summary>
public class WorldMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    private void FixedUpdate()
    {
        Vector3 movement = Vector3.back * moveSpeed * Time.fixedDeltaTime;
        transform.position += movement;
    }

    public void SetMoveSpeed(float newMoveSpeed)
    {
        moveSpeed = newMoveSpeed;
    }
}