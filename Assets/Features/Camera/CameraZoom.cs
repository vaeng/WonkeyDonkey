using UnityEngine;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Camera))]
public class CameraZoom : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform stackRoot;
    [SerializeField] private Transform bottomAnchor;
    [SerializeField] private LayerMask stackItemLayers;

    [Header("Settings")]
    [SerializeField, Range(0.1f, 0.9f)] private float maxStackScreenHeight = 0.4f;
    [SerializeField] private float maxZoomOut = 12f;
    [SerializeField] private float maxLift = 10f;
    [SerializeField] private float moveSpeed = 5f;

    private const float stackCheckWidth = 5f;
    private const float stackCheckHeight = 80f;
    private const int searchSteps = 12;

    private Camera cam;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float startBottomScreenY;

    private void Start()
    {
        cam = GetComponent<Camera>();

        startPosition = transform.position;
        startRotation = transform.rotation;

        if (bottomAnchor != null)
            startBottomScreenY = GetScreenY(bottomAnchor.position, startPosition);
    }

    private void LateUpdate()
    {
        if (stackRoot == null || bottomAnchor == null)
            return;

        Vector3 targetPosition = startPosition;

        if (TryGetStackTop(out Vector3 stackTop))
        {
            bool stackIsTooHigh = GetScreenY(stackTop, startPosition) > maxStackScreenHeight;

            if (stackIsTooHigh)
                targetPosition = FindCameraPosition(stackTop);
        }

        float lerp = 1f - Mathf.Exp(-moveSpeed * Time.deltaTime);
        Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, lerp);

        transform.SetPositionAndRotation(newPosition, startRotation);
    }

    private Vector3 FindCameraPosition(Vector3 stackTop)
    {
        float minZoom = 0f;
        float maxZoom = maxZoomOut;

        for (int i = 0; i < searchSteps; i++)
        {
            float zoom = (minZoom + maxZoom) * 0.5f;
            Vector3 position = PositionForZoom(zoom);

            if (GetScreenY(stackTop, position) > maxStackScreenHeight)
                minZoom = zoom;
            else
                maxZoom = zoom;
        }

        return PositionForZoom(maxZoom);
    }

    private Vector3 PositionForZoom(float zoom)
    {
        Vector3 position = startPosition - startRotation * Vector3.forward * zoom;

        float minLift = 0f;
        float maxLiftValue = maxLift;

        for (int i = 0; i < searchSteps; i++)
        {
            float lift = (minLift + maxLiftValue) * 0.5f;
            Vector3 liftedPosition = position + Vector3.up * lift;

            if (GetScreenY(bottomAnchor.position, liftedPosition) > startBottomScreenY)
                minLift = lift;
            else
                maxLiftValue = lift;
        }

        position.y += maxLiftValue;
        return position;
    }

    private bool TryGetStackTop(out Vector3 stackTop)
    {
        stackTop = stackRoot.position;

        Vector3 checkCenter = stackRoot.position + Vector3.up * stackCheckHeight * 0.5f;
        Vector3 checkSize = new Vector3(stackCheckWidth, stackCheckHeight, stackCheckWidth) * 0.5f;

        Collider[] hits = Physics.OverlapBox(
            checkCenter,
            checkSize,
            Quaternion.identity,
            stackItemLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            if (hit.bounds.max.y <= stackTop.y)
                continue;

            stackTop = new Vector3(
                hit.bounds.center.x,
                hit.bounds.max.y,
                hit.bounds.center.z
            );
        }

        return stackTop.y > stackRoot.position.y;
    }

    private float GetScreenY(Vector3 point, Vector3 cameraPosition)
    {
        Vector3 oldPosition = transform.position;
        Quaternion oldRotation = transform.rotation;

        transform.SetPositionAndRotation(cameraPosition, startRotation);
        float screenY = cam.WorldToViewportPoint(point).y;
        transform.SetPositionAndRotation(oldPosition, oldRotation);

        return screenY;
    }
}