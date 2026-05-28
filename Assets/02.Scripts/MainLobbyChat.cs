using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Client;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private ChatClient chatClient;

    private void Start()
    {
        chatClient = new ChatClient(this);

        AuthenticationValues auth = new AuthenticationValues();
        auth.UserId = "Player_" + Random.Range(1000, 9999);

        chatClient.Connect(chatAppId, appVersion, auth);
    }

    private void Update()
    {
        if (chatClient != null)
        {
            chatClient.Service();
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            SendChat();
        }
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
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < messages.Length; i++)
        {
            TMP_Text chatText = Instantiate(chatTextPrefab, contentParent);
            chatText.text = $"{senders[i]} : {messages[i]}";
        }
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