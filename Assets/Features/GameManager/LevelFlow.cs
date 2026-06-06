using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System;

public class LevelFlow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoadSpawner roadSpawner;
    [SerializeField] private ItemSpawner itemSpawner;
    [SerializeField] private LaneMovement laneMovement;

    [Header("UI")]
    [SerializeField] private GameObject scoreScreen;
    [SerializeField] private Text timerText;

    [Header("End")]
    [SerializeField] private float slowDownTime = 0.5f;
    [SerializeField] private float distanceToStopItemsBeforeFinish = 5f;

    public event Action OnLevelEnded;

    private float timeLeft;
    private float slowDownTimer;

    private bool itemsStopped;
    private bool endingStarted;
    private bool levelEnded;

    private void Start()
    {
        WorldMover.SpeedMultiplier = 1f;

        if (scoreScreen != null)
            scoreScreen.SetActive(false);

        if (roadSpawner != null)
            timeLeft = roadSpawner.LevelDuration;
    }

    private void Update()
    {
        if (levelEnded)
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                RestartLevel();

            return;
        }

        UpdateTimer();
        CheckItemSpawning();

        if (!endingStarted && roadSpawner != null && roadSpawner.HasReachedFinish)
            StartEnding();

        if (endingStarted)
            SlowDownWorld();
    }

    private void UpdateTimer()
    {
        if (endingStarted)
            return;

        timeLeft = Mathf.Max(0f, timeLeft - Time.deltaTime);

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timeLeft).ToString();
    }

    private void CheckItemSpawning()
    {
        if (itemsStopped || roadSpawner == null || itemSpawner == null)
            return;

        float stopAtZ = itemSpawner.SpawnZPosition + distanceToStopItemsBeforeFinish;

        if (roadSpawner.GetFinishSegmentZ() > stopAtZ)
            return;

        itemSpawner.StopSpawning();
        itemsStopped = true;
    }

    private void StartEnding()
    {
        endingStarted = true;
        OnLevelEnded?.Invoke();
        slowDownTimer = 0f;

        if (laneMovement != null)
            laneMovement.enabled = false;
    }

    private void SlowDownWorld()
    {
        slowDownTimer += Time.deltaTime;

        float t = slowDownTimer / slowDownTime;
        WorldMover.SpeedMultiplier = Mathf.Lerp(1f, 0f, t);

        if (slowDownTimer < slowDownTime)
            return;

        WorldMover.SpeedMultiplier = 0f;
        levelEnded = true;

        if (scoreScreen != null)
            scoreScreen.SetActive(true);
    }

    private void RestartLevel()
    {
        WorldMover.SpeedMultiplier = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void OnDestroy()
    {
        WorldMover.SpeedMultiplier = 1f;
    }
}