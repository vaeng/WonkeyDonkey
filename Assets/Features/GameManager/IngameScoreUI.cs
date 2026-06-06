using System.Collections;
using UnityEngine;

public class IngameScoreUI : MonoBehaviour
{
    [SerializeField] private  UnityEngine.UI.Text scoreText;
    Highscore _highscore;
    int currentScore;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int currentScore = 0;
        scoreText.text = currentScore.ToString("F0");
        StartCoroutine(DelayedSubscribtion());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator DelayedSubscribtion()
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
        scoreText.text = currentScore.ToString("F0");
    }

    void OnScoreWentDown(int _amount)
    {
        currentScore -= _amount;
        scoreText.text = currentScore.ToString("F0");
    }
}
