using UnityEngine;

public class RotateWheel : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 360f; // Degrees per second

    public void StartRotation()
    {
        enabled = true;
    }

    public void StopRotation()
    {
        enabled = false;
    }

    private void FixedUpdate()
    {
        if (enabled)
        {
            rotateWheel(Time.fixedDeltaTime);
        }
    }

    void rotateWheel(float deltaTime)
    {
        float rotationAmount = rotationSpeed * deltaTime;
        transform.Rotate(Vector3.right, rotationAmount);
    }
}
