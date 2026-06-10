using System.Collections;
using UnityEngine;

public class FinalScreenManager : MonoBehaviour
{
    [SerializeField] private float delayBeforeMovingBoxes = 1.5f;
    [SerializeField] private float delayBetweenRemovals = 0.3f;
    [SerializeField] private GameObject rewardItemPrefab;
    [SerializeField] private GameObject newHighscorePrefab;
    [SerializeField] private int RewardsPerCrate = 10;
    [SerializeField] private float delayBetweenRewards = 0.1f;
    [SerializeField] private GameObject redItemCollectorBorder;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject inGameScoreDisplay;


    Highscore highscore;
    void OnEnable()
    {
        StartCoroutine(DelayedSubscription());
    }

    void OnDisable()
    {
        var levelFlow = FindAnyObjectByType<LevelFlow>();
        if (levelFlow != null)
        {
            levelFlow.OnLevelEnded -= OnLevelEnded;
        }

    }

    private IEnumerator DelayedSubscription()
    {
        yield return new WaitForSeconds(0.5f);
        var levelFlow = FindAnyObjectByType<LevelFlow>();
        if (levelFlow != null)
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
        // don't show pause
        pauseButton.SetActive(false);
        // don't show score
        inGameScoreDisplay.SetActive(false);
        // show goods lost
        // start with negative score
        // count up
        // show old highscore
        // show new highscore if beaten

        redItemCollectorBorder.SetActive(false);

        yield return new WaitForSeconds(delayBeforeMovingBoxes);

        var possibleRespawnPoints = FindObjectsByType<ItemSpawnPointTag>(FindObjectsInactive.Include);

        while (true)
        {
            var item = FindAnyObjectByType<StackableItemPhysics>();

            if (item == null)
            {
                break;
            }

            if (possibleRespawnPoints.Length == 0)
            {
                Destroy(item.gameObject);
            }
            else
            {
                var randomPoint = possibleRespawnPoints[Random.Range(0, possibleRespawnPoints.Length)].transform.position;
                var rb = item.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.position = randomPoint;
                }
                else
                {
                    item.transform.position = randomPoint;
                }
                Destroy(item.GetComponent<StackableItemPhysics>());

            }

            yield return new WaitForSeconds(delayBetweenRemovals);
        }
        if (highscore != null && highscore.GetTotalScore() >= highscore.GetHighScore())
        {
            newHighscorePrefab.SetActive(true);
        }
        if (highscore != null)
        {
            int rewardCount = highscore.DeliveredCrateCount * RewardsPerCrate;
            var rewardSpawnPoint = FindAnyObjectByType<RewardSpawnPointTag>();
            for (int i = 0; i < rewardCount; i++)
            {
                var randomQuaternion = Random.rotation;
                Instantiate(rewardItemPrefab, rewardSpawnPoint.transform.position, randomQuaternion);
                rewardSpawnPoint.transform.localScale = 0.2f * Vector3.one;
                yield return new WaitForSeconds(delayBetweenRewards);
            }
        }

    }
}
