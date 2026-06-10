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