using System.Collections;
using UnityEngine;
using MoreMountains.Feedbacks;

public class IngameScoreUI : MonoBehaviour
{
    [SerializeField] private  UnityEngine.UI.Text scoreText;
     [Header("Feedbacks")]
    /// a MMF_Player to play when the Hero starts jumping
    public MMF_Player ScoreUpFeedback;
    /// a MMF_Player to play when the Hero lands after a jump
    public MMF_Player ScoreDownFeedback;
    Highscore _highscore;
    int currentScore;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int currentScore = 0;
        scoreText.text = currentScore.ToString("F0");
        StartCoroutine(DelayedSubscription());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator DelayedSubscription()
    {
        yield return new WaitForSeconds(0.5f);
        _highscore = FindAnyObjectByType<Highscore>();
        if(_highscore != null)
        {
            _highscore.OnScoreWentUp += OnScoreWentUp;
            _highscore.OnScoreWentDown += OnScoreWentDown;
        }
    }

    void OnDisable()
    {
        if(_highscore != null)
        {
            _highscore.OnScoreWentUp -= OnScoreWentUp;
            _highscore.OnScoreWentDown -= OnScoreWentDown;
        }
    }

    void OnScoreWentUp(int _amount)
    {
        currentScore += _amount;
        ScoreUpFeedback?.PlayFeedbacks();
        scoreText.text = currentScore.ToString("F0");
    }

    void OnScoreWentDown(int _amount)
    {
        currentScore -= _amount;
        ScoreDownFeedback?.PlayFeedbacks();
        scoreText.text = currentScore.ToString("F0");
    }
}
