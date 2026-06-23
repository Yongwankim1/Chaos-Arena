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
    [Header("Photon Fusion")]
    [SerializeField] private LobbyManager lobby;
    [Header("UserNickNameChecker")]
    [SerializeField] private UserNickNameChecker nickNameChecker;
    [Header("Photon Chat")]
    [SerializeField] private string chatAppId;
    [SerializeField] private string appVersion = "1.0";
    [SerializeField] private string channelName = "MainLobby";

    [Header("UI")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject createPlayerPanel;
    [SerializeField] private Transform contentParent;
    [SerializeField] private TMP_Text chatTextPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform userListParent;
    private ChatClient chatClient;
    private string myNickName;
    private bool isSubscribedToMainChannel;
    private bool isSendingAfterImeCommit;


    [Header("Prefabs")]
    [SerializeField] private TMP_Text userNickName;
    public List<TMP_Text> userNickNames = new List<TMP_Text>();
    void Awake()
    {
        if(nickNameChecker == null) nickNameChecker = GetComponent<UserNickNameChecker>();
    }
    void OnEnable()
    {
        if (nickNameChecker != null) nickNameChecker.OnSetNickName += Connect;
    }
    void OnDisable()
    {
        if (nickNameChecker != null) nickNameChecker.OnSetNickName -= Connect;
    }
    public void Connect()
    {
        if (chatClient != null)
        {
            if (chatClient.State == ChatState.ConnectedToFrontEnd ||
                chatClient.State == ChatState.Authenticated)
            {
                SubscribeMainChannel();
                return;
            }

            if (chatClient.State == ChatState.ConnectingToNameServer)
            {
                Debug.Log("이미 채팅 연결 중이거나 연결됨");
                return;
            }
        }
        TMP_Text text = Instantiate(chatTextPrefab, contentParent);
        text.text = "채널 입장 중...";

        chatClient = new ChatClient(this);

        string nickName = UserDataManager.Instance.UserData.UserName;

        if (string.IsNullOrWhiteSpace(nickName))
        {
            nickName = "Player_" + Random.Range(1000, 9999);
        }
        myNickName = nickName;

        string uniqueUserId = nickName + "|" + System.Guid.NewGuid().ToString("N");

        ChatAppSettings chatAppSettings = new ChatAppSettings();
        chatAppSettings.AppIdChat = chatAppId;
        chatAppSettings.AppVersion = appVersion;

        chatClient.AuthValues = new AuthenticationValues(uniqueUserId);

        bool isConnectStart = chatClient.ConnectUsingSettings(chatAppSettings);
        Debug.Log("Chat Connect Start: " + isConnectStart);
        Debug.Log("Chat NickName: " + nickName);
        Debug.Log("Chat UserId: " + chatClient.UserId);
    }
    public void Unsubscribe()
    {
        if (chatClient == null) return;
        isSubscribedToMainChannel = false;
        chatClient.Unsubscribe(new string[] { channelName });
    }
    private bool IsInputFocused()
    {
        return EventSystem.current.currentSelectedGameObject == inputField.gameObject;
    }

    private bool CanUseChatHotkey()
    {
        if (inputField == null || !inputField.isActiveAndEnabled)
            return false;

        if (createPlayerPanel != null && createPlayerPanel.activeInHierarchy)
            return false;

        if (EventSystem.current == null)
            return true;

        GameObject selectedObject =
            EventSystem.current.currentSelectedGameObject;

        if (selectedObject == null)
            return true;

        if (selectedObject == inputField.gameObject)
            return true;

        return selectedObject.GetComponent<TMP_InputField>() == null;
    }

    private void Update()
    {
        chatClient?.Service();

        if (!CanUseChatHotkey())
            return;

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

            SendChatAfterImeCommit();
        }
    }

    private void SendChatAfterImeCommit()
    {
        if (isSendingAfterImeCommit)
            return;

        StartCoroutine(SendChatAfterImeCommitRoutine());
    }

    private IEnumerator SendChatAfterImeCommitRoutine()
    {
        isSendingAfterImeCommit = true;
        yield return null;

        SendChat();
        isSendingAfterImeCommit = false;
    }

    private IEnumerator ScrollToBottomRoutine()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        scrollRect.verticalNormalizedPosition = 0f;

        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
    }
    public void SendChat()
    {
        if (chatClient == null) return;
        if (string.IsNullOrWhiteSpace(inputField.text)) return;

        string sendMessage = myNickName + "|" + inputField.text;

        chatClient.PublishMessage(channelName, sendMessage);

        inputField.text = "";
        inputField.ActivateInputField();
    }

    public void OnConnected()
    {
        Debug.Log("Photon Chat 서버 연결 성공");
        SubscribeMainChannel();
    }

    private void SubscribeMainChannel()
    {
        if (chatClient == null) return;
        if (isSubscribedToMainChannel) return;

        chatClient.Subscribe( channelName, 0, 0, new ChannelCreationOptions
        {
            PublishSubscribers = true
        });
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log("채널 입장 성공: " + channels[0]);
        isSubscribedToMainChannel = true;
        TMP_Text text = Instantiate(chatTextPrefab, contentParent);
        text.text = "채널 입장!";

        UpdateUserList();
    }
    void UpdateUserList()
    {
        if (chatClient == null) return;
        if (!chatClient.TryGetChannel(channelName, out ChatChannel channel)) return;

        foreach (TMP_Text nickText in userNickNames)
        {
            Destroy(nickText.gameObject);
        }

        userNickNames.Clear();

        foreach (string user in channel.Subscribers)
        {
            TMP_Text text = Instantiate(userNickName, userListParent);

            string[] split = user.Split('|');

            if (split.Length >= 2)
            {
                text.text = split[0];
            }
            else
            {
                text.text = user;
            }

            userNickNames.Add(text);
        }
    }
    [ContextMenu("현재 유저 보기")]
    void PrintUserList()
    {
        if (chatClient == null) return;
        if (!chatClient.TryGetChannel(channelName, out ChatChannel channel)) return;

        foreach (string user in channel.Subscribers)
        {
            Debug.Log("현재 유저: " + user);
        }
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < messages.Length; i++)
        {
            TMP_Text chatText = Instantiate(chatTextPrefab, contentParent);

            string rawMessage = messages[i].ToString();
            string[] split = rawMessage.Split('|');

            if (split.Length >= 2)
            {
                string nickName = split[0];
                string message = split[1];

                chatText.text = $"{nickName} : {message}";
            }
            else
            {
                chatText.text = $"{senders[i]} : {messages[i]}";
            }
        }

        StartCoroutine(ScrollToBottomRoutine());
    }

    public void OnDisconnected()
    {
        Debug.Log("Photon Chat 서버 연결 끊김");
    }

    public void OnChatStateChange(ChatState state) { }
    public void DebugReturn(DebugLevel level, string message) { }
    public void OnUnsubscribed(string[] channels)
    {
        foreach (string channel in channels)
        {
            if (channel == channelName)
            {
                isSubscribedToMainChannel = false;
                break;
            }
        }
    }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }
    public void OnPrivateMessage(string sender, object message, string channelName) { }
    public void OnUserSubscribed(string channel, string user)
    {
        Debug.Log("유저 입장: " + user);
        UpdateUserList();
    }
    public void OnUserUnsubscribed(string channel, string user)
    {
        Debug.Log("유저 퇴장: " + user);
        UpdateUserList();
    }

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
