using UnityEngine;

public class StartRotation : MonoBehaviour
{
    public bool rotateX, rotateY, rotateZ;
    [SerializeField] private float rotationInterval = 90f;

    void Start()
    {
        int range = Mathf.RoundToInt(360f / rotationInterval);
        int randomXRotation = Random.Range(0, range) * 90;
        int randomYRotation = Random.Range(0, range) * 90;
        int randomZRotation = Random.Range(0, range) * 90;

        float rotX = rotateX ? randomXRotation : 0;
        float rotY = rotateY ? randomYRotation : 0;
        float rotZ = rotateZ ? randomZRotation : 0;

        transform.Rotate(rotX, rotY, rotZ);
    }
}
