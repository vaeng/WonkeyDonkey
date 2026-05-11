using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class SwipeInput : MonoBehaviour
{
    public event Action OnSwipeLeft;
    public event Action OnSwipeRight;

    /// <summary>Minimum distance in pixels for a swipe to be registered.</summary>
    [Header("Swipe Settings")]
    [SerializeField] private float minSwipeDistance = 50f;

    /// <summary>Allows using keyboard input for left/right swipes.</summary>
    [Header("Debug / Fallback Input")]
    [SerializeField] private bool allowKeyboardInput = true;

    private Vector2 startPosition;
    private bool isSwiping;

    private void Update()
    {
        if (allowKeyboardInput)
            HandleKeyboardInput();

        HandleMouseInput();
        HandleTouchInput();
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.aKey.wasPressedThisFrame ||
            Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            OnSwipeLeft?.Invoke();
        }

        if (Keyboard.current.dKey.wasPressedThisFrame ||
            Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            OnSwipeRight?.Invoke();
        }
    }

    private void HandleMouseInput()
    {
        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            startPosition = Mouse.current.position.ReadValue();
            isSwiping = true;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isSwiping)
        {
            Vector2 endPosition = Mouse.current.position.ReadValue();
            DetectSwipe(endPosition);
            isSwiping = false;
        }
    }

    private void HandleTouchInput()
    {
        if (Touchscreen.current == null)
            return;

        TouchControl primaryTouch = Touchscreen.current.primaryTouch;

        if (primaryTouch.press.wasPressedThisFrame)
        {
            startPosition = primaryTouch.position.ReadValue();
            isSwiping = true;
        }

        if (primaryTouch.press.wasReleasedThisFrame && isSwiping)
        {
            Vector2 endPosition = primaryTouch.position.ReadValue();
            DetectSwipe(endPosition);
            isSwiping = false;
        }
    }

    private void DetectSwipe(Vector2 endPosition)
    {
        Vector2 swipeDelta = endPosition - startPosition;

        if (Mathf.Abs(swipeDelta.x) < minSwipeDistance)
            return;

        if (Mathf.Abs(swipeDelta.x) < Mathf.Abs(swipeDelta.y))
            return;

        if (swipeDelta.x > 0)
            OnSwipeRight?.Invoke();
        else
            OnSwipeLeft?.Invoke();
    }
}