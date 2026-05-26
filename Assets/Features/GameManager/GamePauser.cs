using UnityEngine;
using UnityEngine.InputSystem;

public class GamePauser : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        if (WasPausePressed())
            PauseOrUnpauseGame();
    }

    private bool WasPausePressed()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                return true;
        }

        return false;
    }

    public void PauseOrUnpauseGame()
    {
        if (Time.timeScale > 0f)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }
}
