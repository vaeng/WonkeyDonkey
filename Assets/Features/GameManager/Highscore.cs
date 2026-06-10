using UnityEngine;
using System.Collections;
using System;

public class Highscore : MonoBehaviour
{
    [Header("Score Tracking")]
    [SerializeField] public int fallenCratePenalty = 50;
    [SerializeField] public int pickedUpReward = 100;
    public int FallenCrateCount { get; private set; }
    public int DeliveredCrateCount { get; private set; }
    public int TotalScore { get; private set; }

    public event Action<int> OnScoreWentUp;
    public event Action<int> OnScoreWentDown;


    void OnEnable()
    {
        FallenCrateCount = 0;
        DeliveredCrateCount = 0;
        TotalScore = 0;
        StartCoroutine(DelayedSubscription());
    }

    void OnDisable()
    {
        StackableItemPhysics.OnItemFallen -= OnCrateFallen;
        var itemCollector = FindAnyObjectByType<ItemCollector>();
        if(itemCollector != null)
        {
            itemCollector.OnItemCollected -= OnCratePickedUp;
        }
    }

    private IEnumerator DelayedSubscription()
    {
        yield return new WaitForSeconds(0.5f);
        StackableItemPhysics.OnItemFallen += OnCrateFallen;
        var itemCollector = FindAnyObjectByType<ItemCollector>();
        if(itemCollector != null)
        {
            itemCollector.OnItemCollected += OnCratePickedUp;
        }
    } 

    public void OnCrateFallen()
    {
        FallenCrateCount++;
        TotalScore -= fallenCratePenalty;
        OnScoreWentDown?.Invoke(fallenCratePenalty);
    }

    public void OnCratePickedUp(CollectedItemInfo _info)
    {
        DeliveredCrateCount++;
        TotalScore += pickedUpReward;
        OnScoreWentUp?.Invoke(pickedUpReward);
    }

    public float GetTotalScore()
    {
        return TotalScore;
    }

    public float GetHighScore()
    {
        var highscore = PlayerPrefs.GetFloat("Highscore", 0);
        var currentScore = GetTotalScore();
        if (currentScore > highscore)
        {
            PlayerPrefs.SetFloat("Highscore", currentScore);
            return currentScore;
        }
        return highscore;
    }
}