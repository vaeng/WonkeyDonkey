using UnityEngine;
using System.Collections;

public class Highscore : MonoBehaviour
{
    [Header("Score Tracking")]
    [SerializeField] private int fallenCratePenalty = 50;
    [SerializeField] private int pickedUpReward = 100;
    public int FallenCrateCount { get; private set; }
    public int DeliveredCrateCount { get; private set; }
    public int TotalScore { get; private set; }

    void OnEnable()
    {
        FallenCrateCount = 0;
        DeliveredCrateCount = 0;
        TotalScore = 0;
        StartCoroutine(DelayedSubscribtion());
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

    private IEnumerator DelayedSubscribtion()
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
    }

    public void OnCratePickedUp(CollectedItemInfo _info)
    {
        DeliveredCrateCount++;
        TotalScore += pickedUpReward;
    }

    public float GetTallestStackHeight()
    {
        var crates = FindObjectsByType<StackableItemPhysics>();
        var height = 0;
        foreach (var crate in crates)
        {
            if (crate.transform.position.y > height)
            {
                height = (int)crate.transform.position.y;
            }
        }
        return height;
    }

    public float GetTotalScore()
    {
        return TotalScore * GetTallestStackHeight();
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