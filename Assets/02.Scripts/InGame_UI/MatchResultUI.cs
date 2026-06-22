using TMPro;
using UnityEngine;

public class MatchResultUI : MonoBehaviour
{
    public static MatchResultUI Instance;

    [SerializeField]
    private TMP_Text resultText;

    [SerializeField]
    private TMP_Text countdownText;

    private float _remainTime;

    private bool _counting;

    private bool _forceReturnLobby;

    private void Awake()
    {
        Instance = this;

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_counting)
            return;

        _remainTime -= Time.unscaledDeltaTime;

        countdownText.text = $"잠시 후 로비로 이동합니다. (<color=#FF4444>{Mathf.CeilToInt(_remainTime)}</color>초)";

        if (_remainTime > 0f)
            return;

        _counting = false;

        if (_forceReturnLobby)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    public void Show(string message, float countdown = 5f, bool forceReturnLobby = false)
    {
        resultText.text = message;

        _remainTime = countdown;

        _counting = true;

        _forceReturnLobby = forceReturnLobby;

        gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;

        Cursor.visible = true;
    }
}