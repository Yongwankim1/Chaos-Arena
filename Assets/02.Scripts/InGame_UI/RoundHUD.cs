using TMPro;
using UnityEngine;

public class RoundHUD : MonoBehaviour
{
    public static RoundHUD Instance
    {
        get;
        private set;
    }

    [SerializeField]
    private TMP_Text timerText;

    [SerializeField]
    private TMP_Text[] killText;
    [SerializeField]
    private TMP_Text[] scoreText;
    [SerializeField]
    private GameObject roundResultTextPanel;
    [SerializeField]
    private TMP_Text roundResultText;
    [SerializeField]
    private TMP_Text roundText;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (RoundManager.Instance == null)
            return;

        if (!RoundManager.Instance.Object)
            return;

        if (!RoundManager.Instance.Object.IsValid)
            return;

        UpdateTimer();
        UpdateScore();
        UpdateRound();
        UpdateRoundResult();
    }
    private void UpdateRound()
    {
        RoundManager round = RoundManager.Instance;

        roundText.text = $"ROUND {round.CurrentRound}";
    }

    private void UpdateScore()
    {
        RoundManager round = RoundManager.Instance;

        killText[0].text = round.BlueScore.ToString();
        scoreText[0].text = round.BlueRoundWin.ToString();

        killText[1].text = round.RedScore.ToString();
        scoreText[1].text = round.RedRoundWin.ToString();
    }

    private void UpdateRoundResult()
    {
        RoundManager round = RoundManager.Instance;

        switch (round.RoundResult)
        {
            case RoundResultType.None:

                roundResultTextPanel.SetActive(false);
                break;

            case RoundResultType.BlueWin:

                roundResultTextPanel.SetActive(true);

                roundResultText.text ="ºí·çÆÀ ½Â¸®";

                break;

            case RoundResultType.RedWin:

                roundResultTextPanel.SetActive(true);

                roundResultText.text = "·¹µåÆÀ ½Â¸®";

                break;

            case RoundResultType.Draw:

                roundResultTextPanel.SetActive(true);

                roundResultText.text = "¹«½ÂºÎ";

                break;
        }
    }
    private void UpdateTimer()
    {
        RoundManager round = RoundManager.Instance;

        float remainTime = Mathf.Max(0f, round.StateRemainTime);

        switch (round.CurrentState)
        {
            case RoundState.Waiting:

                timerText.text = $"{Mathf.CeilToInt(remainTime)}";

                break;

            case RoundState.CharacterSelect:

                timerText.text = $"{Mathf.CeilToInt(remainTime)}";

                break;

            case RoundState.Preparation:

                timerText.text = $"{Mathf.CeilToInt(remainTime)}";

                break;

            case RoundState.Playing:

                int minutes = Mathf.FloorToInt(remainTime / 60);

                int seconds = Mathf.FloorToInt(remainTime % 60);

                timerText.text = $"{minutes:00}:{seconds:00}";

                break;

            case RoundState.RoundEnd:

                timerText.text = "¶ó¿îµå Á¾·á";

                break;
        }
    }
}