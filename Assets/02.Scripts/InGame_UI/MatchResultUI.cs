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

    private void Awake()
    {
        Instance = this;

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_counting)
            return;

        _remainTime -= Time.deltaTime;

        countdownText.text = $"잠시 후 로비로 이동합니다. (<color=#FF4444>{Mathf.CeilToInt(_remainTime)}</color>초)";

        if (_remainTime <= 0f)
        {
            _counting = false;
        }
    }

    public void Show(string message,float countdown = 5f)
    {
        resultText.text = message;

        _remainTime = countdown;

        _counting = true;

        gameObject.SetActive(true);

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;
    }
}