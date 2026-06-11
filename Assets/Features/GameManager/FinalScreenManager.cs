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
    [SerializeField] private GameObject ReplayButton;

    [SerializeField] private UnityEngine.UI.Text GoodsLostDisplayText;
    [SerializeField] private UnityEngine.UI.Text GoodsLostDisplayNumber;
    [SerializeField] private UnityEngine.UI.Text GoodsDeliveredDisplayText;
    [SerializeField] private UnityEngine.UI.Text GoodsDeliveredDisplayNumber;
    [SerializeField] private UnityEngine.UI.Text CurrentHighscoreDisplayText;
    [SerializeField] private UnityEngine.UI.Text CurrentHighscoreDisplayNumber;


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
        // deactivate all
        GoodsLostDisplayText.gameObject.SetActive(false);
        GoodsLostDisplayNumber.gameObject.SetActive(false);
        GoodsDeliveredDisplayText.gameObject.SetActive(false);
        GoodsDeliveredDisplayNumber.gameObject.SetActive(false);
        CurrentHighscoreDisplayText.gameObject.SetActive(false);
        CurrentHighscoreDisplayNumber.gameObject.SetActive(false);
        redItemCollectorBorder.SetActive(false);
        // don't show pause
        pauseButton.SetActive(false);
        // don't show score
        inGameScoreDisplay.SetActive(false);
        // show goods lost
        yield return new WaitForSeconds(0.2f);
        GoodsLostDisplayText.gameObject.SetActive(true);
        GoodsLostDisplayNumber.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.2f);
        var ingameScoreUI = inGameScoreDisplay.GetComponent<IngameScoreUI>();
        if (ingameScoreUI == null)
        {
            Debug.LogError("IngameScoreUI component not found on inGameScoreDisplay object.");
            yield break;
        }
        ingameScoreUI.ResetScore();
        ingameScoreUI.gameObject.SetActive(true);
        inGameScoreDisplay.SetActive(true);
        GoodsLostDisplayNumber.text = 0.ToString();
        for (int i = 0; i < highscore.FallenCrateCount; i++)
        {
            // count up
            ingameScoreUI.OnScoreWentDown(highscore.fallenCratePenalty);
            GoodsLostDisplayNumber.text = (i + 1).ToString();
            yield return new WaitForSeconds(0.2f);
        }

        
        yield return new WaitForSeconds(0.4f);


        yield return new WaitForSeconds(delayBeforeMovingBoxes);
        GoodsDeliveredDisplayText.gameObject.SetActive(true);
        int deliveredItemsCounter = 0;
        GoodsDeliveredDisplayNumber.text = deliveredItemsCounter.ToString("N0");
        GoodsDeliveredDisplayNumber.gameObject.SetActive(true);

        var possibleRespawnPoints = FindObjectsByType<ItemSpawnPointTag>(FindObjectsInactive.Include);

        while (true)
        {
            var item = FindAnyObjectByType<StackableItemPhysics>();

            if (item == null)
            {
                break;
            }
            // count up
            deliveredItemsCounter++;
            ingameScoreUI.OnScoreWentUp(highscore.pickedUpReward);
            GoodsDeliveredDisplayNumber.text = deliveredItemsCounter.ToString("N0");

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

        // show new highscore if beaten
        if (highscore != null && highscore.GetTotalScore() >= highscore.GetHighScore())
        {
            CurrentHighscoreDisplayText.text = "New\nHighscore!";
            newHighscorePrefab.SetActive(true);
        }
        // show old highscore
        CurrentHighscoreDisplayNumber.gameObject.SetActive(true);
        CurrentHighscoreDisplayText.gameObject.SetActive(true);
        if (highscore != null)
        {
            CurrentHighscoreDisplayNumber.text = highscore.GetHighScore().ToString("N0");
            int rewardCount = Mathf.Min(highscore.DeliveredCrateCount, 2*deliveredItemsCounter - highscore.FallenCrateCount)  * RewardsPerCrate;
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
