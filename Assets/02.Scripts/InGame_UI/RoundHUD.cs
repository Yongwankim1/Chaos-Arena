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
    private TMP_Text roundResultTextPanel;
    [SerializeField]
    private TMP_Text roundResultText;


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
        UpdateRoundResult();
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
        RoundManager round =
            RoundManager.Instance;

        switch (round.RoundResult)
        {
            case RoundResultType.None:

                roundResultTextPanel.gameObject.SetActive(false);
                break;

            case RoundResultType.BlueWin:

                roundResultTextPanel.gameObject.SetActive(true);

                roundResultText.text ="블루팀 승리";

                break;

            case RoundResultType.RedWin:

                roundResultTextPanel.gameObject
                    .SetActive(true);

                roundResultText.text = "레드팀 승리";

                break;

            case RoundResultType.Draw:

                roundResultTextPanel.gameObject.SetActive(true);

                roundResultText.text = "무승부";

                break;
        }
    }
    private void UpdateTimer()
    {
        RoundManager round =
            RoundManager.Instance;

        switch (round.CurrentState)
        {
            case RoundState.Waiting:

                timerText.text =
                    $"잠시만 기다려주세요\n{Mathf.CeilToInt(round.StateRemainTime)}";

                break;

            case RoundState.CharacterSelect:

                timerText.text =
                    $"캐릭터 선택 시간\n{Mathf.CeilToInt(round.StateRemainTime)}";

                break;

            case RoundState.Preparation:

                timerText.text =
                    $"대기\n{Mathf.CeilToInt(round.StateRemainTime)}";

                break;

            case RoundState.Playing:

                int minutes =
                    Mathf.FloorToInt(
                        round.StateRemainTime / 60);

                int seconds =
                    Mathf.FloorToInt(
                        round.StateRemainTime % 60);

                timerText.text =
                    $"{minutes:00}:{seconds:00}";

                break;

            case RoundState.RoundEnd:

                timerText.text =
                    "라운드 종료";

                break;
        }
    }
}