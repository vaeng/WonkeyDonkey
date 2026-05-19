using UnityEngine;

public class CameraViewSettings : MonoBehaviour
{
    public enum CameraView
    {
        BackView,
        FrontView
    }

    [Header("Camera View")]
    [SerializeField] private CameraView selectedView = CameraView.BackView;
    [SerializeField] private bool applyInEditor = true;

    [Header("Input")]
    [SerializeField] private SwipeInput swipeInput;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs;

    private void Start()
    {
        ApplyView();
    }

    private void OnValidate()
    {
        if (applyInEditor)
            ApplyView();
    }

    public void ToggleView()
    {
        if (selectedView == CameraView.BackView)
            selectedView = CameraView.FrontView;
        else
            selectedView = CameraView.BackView;

        ApplyView();
    }

    [ContextMenu("Apply Camera View")]
    public void ApplyView()
    {
        switch (selectedView)
        {
            case CameraView.BackView:
                SetBackView();
                break;

            case CameraView.FrontView:
                SetFrontView();
                break;
        }

        if (showDebugLogs)
            Debug.Log("Current camera view: " + selectedView);
    }

    private void SetBackView()
    {
        transform.position = new Vector3(0f, 5f, -6f);
        transform.rotation = new Quaternion(0.0872f, 0f, 0f, 0.9962f);
        transform.localScale = Vector3.one;

        if (swipeInput != null)
            swipeInput.SetInvertHorizontalInput(false);
    }

    private void SetFrontView()
    {
        transform.position = new Vector3(1.5f, 4f, 20f);
        transform.rotation = new Quaternion(0.0038f, -0.9952f, 0.0871f, 0.0435f);
        transform.localScale = Vector3.one;

        if (swipeInput != null)
            swipeInput.SetInvertHorizontalInput(true);
    }
}