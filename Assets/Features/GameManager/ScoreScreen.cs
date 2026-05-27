using UnityEngine;
using UnityEngine.UI;
public class ScoreScreen : MonoBehaviour
{

    [Header("UI References")]
    [SerializeField] private Text TotalScoreText;
    [SerializeField] private Text GoodsLostText;
    [SerializeField] private Text GoodsDeliveredText;
    [SerializeField] private Text BiggestStackText;

    private Highscore highscore;

    void OnEnable()
    {
        highscore = FindAnyObjectByType<Highscore>();
        if (highscore != null)
        {

            TotalScoreText.text = highscore.TotalScore.ToString();
            GoodsLostText.text = highscore.FallenCrateCount.ToString();
            GoodsDeliveredText.text = highscore.DeliveredCrateCount.ToString();
            BiggestStackText.text = highscore.GetTallestStackHeight().ToString("F2") + "m";
        }
    }

}
