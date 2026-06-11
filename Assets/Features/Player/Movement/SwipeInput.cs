using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class SwipeInput : MonoBehaviour
{
    public event Action OnSwipeLeft;
    public event Action OnSwipeRight;
    public event Action OnSwipeReleased;

    [Header("Swipe")]
    [SerializeField] private float minSwipeDistance = 50f;
    [SerializeField] private float directionChangeDistance = 25f;

    [Header("Input Direction")]
    [SerializeField] private bool invertHorizontalInput;

    [Header("Debug")]
    [SerializeField] private bool useKeyboardInput = true;

    private Vector2 startPosition;
    private Vector2 lastPosition;
    private bool hasStartedSwipe;

    private int currentSwipeDirection;
    private int currentKeyboardDirection;

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

        int keyboardDirection = 0;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            keyboardDirection = -1;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            keyboardDirection = 1;

        if (keyboardDirection == currentKeyboardDirection)
            return;

        currentKeyboardDirection = keyboardDirection;

        if (keyboardDirection < 0)
            SwipeLeft();
        else if (keyboardDirection > 0)
            SwipeRight();
        else
            OnSwipeReleased?.Invoke();
    }

    private void CheckMouse()
    {
        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            StartSwipe(Mouse.current.position.ReadValue());

        if (Mouse.current.leftButton.isPressed)
            CheckSwipe(Mouse.current.position.ReadValue());

        if (Mouse.current.leftButton.wasReleasedThisFrame)
            EndSwipe();
    }

    private void CheckTouch()
    {
        if (Touchscreen.current == null)
            return;

        TouchControl touch = Touchscreen.current.primaryTouch;

        if (touch.press.wasPressedThisFrame)
            StartSwipe(touch.position.ReadValue());

        if (touch.press.isPressed)
            CheckSwipe(touch.position.ReadValue());

        if (touch.press.wasReleasedThisFrame)
            EndSwipe();
    }

    private void StartSwipe(Vector2 position)
    {
        startPosition = position;
        lastPosition = position;
        hasStartedSwipe = true;
        currentSwipeDirection = 0;
    }

    private void CheckSwipe(Vector2 currentPosition)
    {
        if (!hasStartedSwipe)
            return;

        if (currentSwipeDirection == 0)
        {
            CheckFirstSwipeDirection(currentPosition);
            return;
        }

        CheckSwipeDirectionChange(currentPosition);
    }

    private void CheckFirstSwipeDirection(Vector2 currentPosition)
    {
        Vector2 swipe = currentPosition - startPosition;

        float xDistance = Mathf.Abs(swipe.x);
        float yDistance = Mathf.Abs(swipe.y);

        if (xDistance < minSwipeDistance)
            return;

        if (xDistance <= yDistance)
            return;

        if (swipe.x < 0f)
        {
            currentSwipeDirection = -1;
            lastPosition = currentPosition;
            SwipeLeft();
        }
        else
        {
            currentSwipeDirection = 1;
            lastPosition = currentPosition;
            SwipeRight();
        }
    }

    private void CheckSwipeDirectionChange(Vector2 currentPosition)
    {
        float xDifference = currentPosition.x - lastPosition.x;

        if (Mathf.Abs(xDifference) < directionChangeDistance)
            return;

        int newDirection = xDifference < 0f ? -1 : 1;
        lastPosition = currentPosition;

        if (newDirection == currentSwipeDirection)
            return;

        currentSwipeDirection = newDirection;

        if (newDirection < 0)
            SwipeLeft();
        else
            SwipeRight();
    }

    private void EndSwipe()
    {
        if (!hasStartedSwipe)
            return;

        hasStartedSwipe = false;
        currentSwipeDirection = 0;

        OnSwipeReleased?.Invoke();
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