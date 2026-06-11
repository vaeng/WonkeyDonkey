using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


// TODO: Später mit Game States arbeiten (also StartScreen, Playing, Paused, ScoreScreen, etc.)
public class StartScreen : MonoBehaviour
{
    [SerializeField] private GameObject startScreen;
        [SerializeField] private MusicManager musicManager;

    private bool started;

    private void Start()
    {
        Time.timeScale = 0f;

        if (startScreen != null)
            startScreen.SetActive(true);

        musicManager?.PlayMenuMusic();
    }

    private void Update()
    {
        if (started)
            return;

        if (WasStartPressed())
            StartGame();
    }

    private bool WasStartPressed()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                return true;

            if (Keyboard.current.enterKey.wasPressedThisFrame)
                return true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return true;

        return false;
    }

    public void StartGame()
    {
        musicManager.PlayLevelMusic();
        started = true;
        Time.timeScale = 1f;

        if (startScreen != null)
            startScreen.SetActive(false);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.Scene currentScene = SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.name);
        musicManager.StopMusic();
    }
}