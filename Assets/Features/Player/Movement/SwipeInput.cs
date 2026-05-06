using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Detects horizontal swipe gestures and raises events when a left or right swipe is performed.
/// </summary>
/// <remarks>This component can be attached to a GameObject to enable swipe detection using either 
/// touch input on mobile devices or mouse input in the Unity Editor. 
/// The minimum swipe distance required to trigger a swipe event can be configured in the Inspector. 
/// Only horizontal swipes are detected; vertical or diagonal swipes are ignored.
/// </remarks>
public class SwipeInput : MonoBehaviour
{
    public event Action OnSwipeLeft;
    public event Action OnSwipeRight;

    [SerializeField] private float minSwipeDistance = 50f;

    private Vector2 startPosition;
    private bool isSwiping;

    private void Update()
    {
        HandleKeyboardInput();
        HandleMouseInput();
        HandleTouchInput();
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            Debug.Log("Keyboard Left erkannt.");
            OnSwipeLeft?.Invoke();
        }

        if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            Debug.Log("Keyboard Right erkannt.");
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

        Debug.Log("Swipe Delta: " + swipeDelta);

        if (Mathf.Abs(swipeDelta.x) < minSwipeDistance)
        {
            Debug.Log("Swipe zu kurz.");
            return;
        }

        if (Mathf.Abs(swipeDelta.x) < Mathf.Abs(swipeDelta.y))
        {
            Debug.Log("Swipe war eher vertikal.");
            return;
        }

        if (swipeDelta.x > 0)
        {
            Debug.Log("Swipe Right erkannt.");
            OnSwipeRight?.Invoke();
        }
        else
        {
            Debug.Log("Swipe Left erkannt.");
            OnSwipeLeft?.Invoke();
        }
    }
}
