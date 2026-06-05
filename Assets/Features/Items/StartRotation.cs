using UnityEngine;

public class StartRotation : MonoBehaviour
{
    [SerializeField] private Vector3 startRotation;
    public bool randomRotateX, randomRotateY, randomRotateZ;
    [SerializeField] private float randomRotationInterval = 90f;

    void OnEnable()
    {
        transform.rotation = Quaternion.Euler(startRotation);

        if (randomRotationInterval == 0f)
            return;

        int range = Mathf.RoundToInt(360f / randomRotationInterval);
        int randomXRotation = Random.Range(0, range) * 90;
        int randomYRotation = Random.Range(0, range) * 90;
        int randomZRotation = Random.Range(0, range) * 90;

        float rotX = randomRotateX ? randomXRotation : 0;
        float rotY = randomRotateY ? randomYRotation : 0;
        float rotZ = randomRotateZ ? randomZRotation : 0;

        transform.Rotate(rotX, rotY, rotZ);
    }
}
