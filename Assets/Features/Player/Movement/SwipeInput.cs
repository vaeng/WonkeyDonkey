using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class SwipeInput : MonoBehaviour
{
    public event Action OnSwipeLeft;
    public event Action OnSwipeRight;

    [Header("Swipe")]
    [SerializeField] private float minSwipeDistance = 50f;

    [Header("Input Direction")]
    [SerializeField] private bool invertHorizontalInput;

    [Header("Debug")]
    [SerializeField] private bool useKeyboardInput = true;

    private Vector2 startPosition;
    private bool hasStartedSwipe;

    public void SetInvertHorizontalInput(bool shouldInvert)
    {
        invertHorizontalInput = shouldInvert;
    }

    private void Update()
    {
        if (useKeyboardInput)
            CheckKeyboard();

        CheckMouse();
        CheckTouch();
    }

    private void CheckKeyboard()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
            SwipeLeft();

        if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
            SwipeRight();
    }

    private void CheckMouse()
    {
        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            startPosition = Mouse.current.position.ReadValue();
            hasStartedSwipe = true;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            CheckSwipe(Mouse.current.position.ReadValue());
        }
    }

    private void CheckTouch()
    {
        if (Touchscreen.current == null)
            return;

        TouchControl touch = Touchscreen.current.primaryTouch;

        if (touch.press.wasPressedThisFrame)
        {
            startPosition = touch.position.ReadValue();
            hasStartedSwipe = true;
        }

        if (touch.press.wasReleasedThisFrame)
        {
            CheckSwipe(touch.position.ReadValue());
        }
    }

    private void CheckSwipe(Vector2 endPosition)
    {
        if (!hasStartedSwipe)
            return;

        Vector2 swipe = endPosition - startPosition;
        hasStartedSwipe = false;

        float xDistance = Mathf.Abs(swipe.x);
        float yDistance = Mathf.Abs(swipe.y);

        if (xDistance < minSwipeDistance)
            return;

        if (xDistance <= yDistance)
            return;

        if (swipe.x < 0f)
            SwipeLeft();
        else
            SwipeRight();
    }

    private void SwipeLeft()
    {
        if (invertHorizontalInput)
            OnSwipeRight?.Invoke();
        else
            OnSwipeLeft?.Invoke();
    }

    private void SwipeRight()
    {
        if (invertHorizontalInput)
            OnSwipeLeft?.Invoke();
        else
            OnSwipeRight?.Invoke();
    }
}