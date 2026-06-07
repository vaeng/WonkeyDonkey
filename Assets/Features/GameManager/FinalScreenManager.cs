using System.Collections;
using UnityEngine;

public class FinalScreenManager : MonoBehaviour
{
    [SerializeField] private float delayBeforeMovingBoxes = 1.5f;
    [SerializeField] private float delayBetweenRemovals = 0.3f;
    [SerializeField] private GameObject rewardItemPrefab;
    [SerializeField] private int RewardsPerCrate = 10;
    [SerializeField] private float delayBetweenRewards = 0.1f;
    [SerializeField] private GameObject redItemCollectorBorder;

    Highscore highscore;
    void OnEnable()
    {
        StartCoroutine(DelayedSubscription());
    }

    void OnDisable()
    {
        var levelFlow = FindAnyObjectByType<LevelFlow>();
        if(levelFlow != null)
        {
            levelFlow.OnLevelEnded -= OnLevelEnded;
        }

    }

    private IEnumerator DelayedSubscription()
    {
        yield return new WaitForSeconds(0.5f);
        var levelFlow = FindAnyObjectByType<LevelFlow>();
        if(levelFlow != null)
        {
            levelFlow.OnLevelEnded += OnLevelEnded;
        }
        highscore = FindAnyObjectByType<Highscore>();
    } 

    void OnLevelEnded()
    {
        StartCoroutine(StartLevelEndSequence());
    }


    private IEnumerator StartLevelEndSequence()
    {
        redItemCollectorBorder.SetActive(false);
        yield return new WaitForSeconds(delayBeforeMovingBoxes);
        var possibleRespawnPoints = FindObjectsByType<ItemSpawnPointTag>(FindObjectsInactive.Include);
        var collectedItems = FindObjectsByType<StackableItemPhysics>();

        foreach(var item in collectedItems)
        {
            if(item)
            {
                if(item.IsDelivered)
                {
                    continue;
                }
                item.IsDelivered = true;

                if(possibleRespawnPoints.Length == 0)
                {
                    Destroy(item.gameObject);
                    continue;
                }
                var randomPoint = possibleRespawnPoints[Random.Range(0, possibleRespawnPoints.Length)].transform.position;
                item.transform.position = randomPoint;
                yield return new WaitForSeconds(delayBetweenRemovals);
            }
        }

        if(highscore != null)
        {
            int rewardCount = highscore.DeliveredCrateCount * RewardsPerCrate;
            var rewardSpawnPoint = FindAnyObjectByType<RewardSpawnPointTag>();
            for(int i = 0; i < rewardCount; i++)
            {
                var randomQuaternion = Random.rotation;
                Instantiate(rewardItemPrefab, rewardSpawnPoint.transform.position, randomQuaternion);
                rewardSpawnPoint.transform.localScale = 0.2f * Vector3.one;
                yield return new WaitForSeconds(delayBetweenRewards);
            }
        }

    } 
}
