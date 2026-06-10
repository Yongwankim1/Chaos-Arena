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
    private TMP_Text scoreText;
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
        RoundManager round =
            RoundManager.Instance;

        scoreText.text =
            $"블루팀 {round.BlueScore} : {round.RedScore} 레드팀\n" +
            $"Round {round.BlueRoundWin} : {round.RedRoundWin}";
    }

    private void UpdateRoundResult()
    {
        RoundManager round =
            RoundManager.Instance;

        switch (round.RoundResult)
        {
            case RoundResultType.None:

                roundResultText.gameObject
                    .SetActive(false);

                break;

            case RoundResultType.BlueWin:

                roundResultText.gameObject
                    .SetActive(true);

                roundResultText.text =
                    "블루팀 승리";

                break;

            case RoundResultType.RedWin:

                roundResultText.gameObject
                    .SetActive(true);

                roundResultText.text =
                    "레드팀 승리";

                break;

            case RoundResultType.Draw:

                roundResultText.gameObject
                    .SetActive(true);

                roundResultText.text =
                    "무승부";

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