using UnityEngine;
using UnityEngine.UI;
public class ScoreScreen : MonoBehaviour
{

    [Header("UI References")]
    [SerializeField] private Text TotalScoreText;
    [SerializeField] private Text GoodsLostText;
    [SerializeField] private Text GoodsDeliveredText;
    [SerializeField] private Text HighscoreText;

    private Highscore highscore;

    void OnEnable()
    {
        highscore = FindAnyObjectByType<Highscore>();
        if (highscore != null)
        {

            TotalScoreText.text = highscore.GetTotalScore().ToString("F0");
            GoodsLostText.text = highscore.FallenCrateCount.ToString();
            GoodsDeliveredText.text = highscore.DeliveredCrateCount.ToString();
            HighscoreText.text = highscore.GetHighScore().ToString("F0");
        }
    }

}
