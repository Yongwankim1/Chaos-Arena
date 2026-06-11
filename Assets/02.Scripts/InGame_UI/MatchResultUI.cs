using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;

public class MatchResultUI : MonoBehaviour
{
    public static MatchResultUI Instance;

    [SerializeField]
    private TMP_Text resultText;

    [SerializeField]
    private Button lobbyButton;

    private void Awake()
    {
        Instance = this;

        gameObject.SetActive(false);

        lobbyButton.onClick.AddListener(OnClickLobby);
    }

    public void Show(string message)
    {
        resultText.text = message;

        gameObject.SetActive(true);
    }


    public void OnClickLobby()
    {
        NetworkRunner runner = FindFirstObjectByType<NetworkRunner>();

        if (runner == null)
            return;

        runner.LoadScene(SceneRef.FromIndex(0));
    }
}