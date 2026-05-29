using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Client;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class MainLobbyChat : MonoBehaviour, IChatClientListener
{
    [Header("Photon Chat")]
    [SerializeField] private string chatAppId;
    [SerializeField] private string appVersion = "1.0";
    [SerializeField] private string channelName = "MainLobby";

    [Header("UI")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Transform contentParent;
    [SerializeField] private TMP_Text chatTextPrefab;
    [SerializeField] private ScrollRect scrollRect;
    private ChatClient chatClient;


    public void Connect()
    {
        TMP_Text text = Instantiate(chatTextPrefab, contentParent);
        text.text = "채널 입장 중...";
        chatClient = new ChatClient(this);

        string userName = UserDataManager.Instance.UserData.UserName;

        if (string.IsNullOrWhiteSpace(userName))
        {
            userName = "Player_" + Random.Range(1000, 9999);
        }

        ChatAppSettings chatAppSettings = new ChatAppSettings();
        chatAppSettings.AppIdChat = chatAppId;
        chatAppSettings.AppVersion = appVersion;

        chatClient.AuthValues = new AuthenticationValues(userName);

        bool isConnectStart = chatClient.ConnectUsingSettings(chatAppSettings);

        Debug.Log("Chat Connect Start: " + isConnectStart);
        Debug.Log("Chat UserId: " + chatClient.UserId);
    }

    private bool IsInputFocused()
    {
        return EventSystem.current.currentSelectedGameObject == inputField.gameObject;
    }
    private void Update()
    {
        chatClient?.Service();

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (!IsInputFocused())
            {
                inputField.ActivateInputField();
                return;
            }

            if (string.IsNullOrWhiteSpace(inputField.text))
            {
                EventSystem.current.SetSelectedGameObject(null);
                return;
            }

            SendChat();
        }
    }
    private IEnumerator ScrollToBottomRoutine()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        scrollRect.verticalNormalizedPosition = 0f;

        Canvas.ForceUpdateCanvases();
    }
    public void SendChat()
    {
        if (chatClient == null) return;
        if (string.IsNullOrWhiteSpace(inputField.text)) return;

        chatClient.PublishMessage(channelName, inputField.text);

        inputField.text = "";
        inputField.ActivateInputField();
    }

    public void OnConnected()
    {
        Debug.Log("Photon Chat 서버 연결 성공");
        chatClient.Subscribe(new string[] { channelName });
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log("채널 입장 성공: " + channels[0]);
        TMP_Text text = Instantiate(chatTextPrefab, contentParent);
        text.text = "채널 입장!";
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < messages.Length; i++)
        {
            TMP_Text chatText = Instantiate(chatTextPrefab, contentParent);
            chatText.text = $"{senders[i]} : {messages[i]}";
        }
        StartCoroutine(ScrollToBottomRoutine());
    }

    public void OnDisconnected()
    {
        Debug.Log("Photon Chat 서버 연결 끊김");
    }

    public void OnChatStateChange(ChatState state) { }
    public void DebugReturn(DebugLevel level, string message) { }
    public void OnUnsubscribed(string[] channels) { }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }
    public void OnPrivateMessage(string sender, object message, string channelName) { }
    public void OnUserSubscribed(string channel, string user) { }
    public void OnUserUnsubscribed(string channel, string user) { }

    public void DebugReturn(LogLevel level, string message)
    {
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
    }
}