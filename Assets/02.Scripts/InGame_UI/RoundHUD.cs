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
            $"Blue {round.BlueScore} : {round.RedScore} Red\n" +
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
                    "BLUE WIN";

                break;

            case RoundResultType.RedWin:

                roundResultText.gameObject
                    .SetActive(true);

                roundResultText.text =
                    "RED WIN";

                break;

            case RoundResultType.Draw:

                roundResultText.gameObject
                    .SetActive(true);

                roundResultText.text =
                    "DRAW";

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
                    $"Waiting\n{Mathf.CeilToInt(round.StateRemainTime)}";

                break;

            case RoundState.CharacterSelect:

                timerText.text =
                    $"Character Select\n{Mathf.CeilToInt(round.StateRemainTime)}";

                break;

            case RoundState.Preparation:

                timerText.text =
                    $"Preparation\n{Mathf.CeilToInt(round.StateRemainTime)}";

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
                    "Round End";

                break;
        }
    }
}