using Photon.Chat;
using Photon.Client;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RoomChat : MonoBehaviour, IChatClientListener
{
    [Header("Photon Chat")]
    [SerializeField] private string chatAppId;
    [SerializeField] private string appVersion = "1.0";
    [SerializeField] private string roomChannelPrefix = "Room_";

    [Header("UI")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Transform contentParent;
    [SerializeField] private TMP_Text chatTextPrefab;
    [SerializeField] private ScrollRect scrollRect;

    private ChatClient chatClient;
    private string myNickName;
    private string currentChannelName;
    private bool isSubscribed;
    private bool isSendingAfterImeCommit;

    private void OnApplicationQuit()
    {
        if (chatClient == null)
            return;

        chatClient.Disconnect();
        chatClient = null;
        isSubscribed = false;
        currentChannelName = null;
    }

    private void Update()
    {
        chatClient?.Service();

        if (inputField == null || Keyboard.current == null)
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

    public void ConnectToRoom(string roomName)
    {
        if (string.IsNullOrWhiteSpace(roomName))
            return;

        currentChannelName = roomChannelPrefix + roomName;
        myNickName = UserDataManager.Instance.UserData.UserName;

        if (string.IsNullOrWhiteSpace(myNickName))
        {
            myNickName = "Player_" + Random.Range(1000, 9999);
        }

        ClearChatUI();

        if (chatClient != null)
        {
            if (chatClient.State == ChatState.ConnectedToFrontEnd ||
                chatClient.State == ChatState.Authenticated)
            {
                SubscribeRoomChannel();
                return;
            }

            if (chatClient.State == ChatState.ConnectingToNameServer)
                return;
        }

        chatClient = new ChatClient(this);
        chatClient.AuthValues =
            new AuthenticationValues(myNickName + "|" + System.Guid.NewGuid().ToString("N"));

        ChatAppSettings chatAppSettings = new ChatAppSettings
        {
            AppIdChat = chatAppId,
            AppVersion = appVersion
        };

        chatClient.ConnectUsingSettings(chatAppSettings);
    }

    public void LeaveRoom()
    {
        if (chatClient == null || string.IsNullOrEmpty(currentChannelName))
            return;

        isSubscribed = false;
        chatClient.Unsubscribe(new[] { currentChannelName });
        currentChannelName = null;
        ClearChatUI();
    }

    public void SendChat()
    {
        if (chatClient == null) return;
        if (!isSubscribed) return;
        if (string.IsNullOrWhiteSpace(inputField.text)) return;

        string sendMessage = myNickName + "|" + inputField.text;

        chatClient.PublishMessage(currentChannelName, sendMessage);

        inputField.text = "";
        inputField.ActivateInputField();
    }

    private void SubscribeRoomChannel()
    {
        if (chatClient == null) return;
        if (isSubscribed) return;
        if (string.IsNullOrEmpty(currentChannelName)) return;

        chatClient.Subscribe(currentChannelName, 0, 0, new ChannelCreationOptions
        {
            PublishSubscribers = false
        });
    }

    private bool IsInputFocused()
    {
        return EventSystem.current != null &&
               EventSystem.current.currentSelectedGameObject == inputField.gameObject;
    }

    private void SendChatAfterImeCommit()
    {
        if (isSendingAfterImeCommit)
            return;

        StartCoroutine(SendChatAfterImeCommitRoutine());
    }

    private System.Collections.IEnumerator SendChatAfterImeCommitRoutine()
    {
        isSendingAfterImeCommit = true;
        yield return null;

        SendChat();
        isSendingAfterImeCommit = false;
    }

    private System.Collections.IEnumerator ScrollToBottomRoutine()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }

        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
    }

    private void AddChatText(string message)
    {
        if (chatTextPrefab == null || contentParent == null)
            return;

        TMP_Text chatText = Instantiate(chatTextPrefab, contentParent);
        chatText.text = message;
    }

    private void ClearChatUI()
    {
        if (contentParent == null)
            return;

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnConnected()
    {
        SubscribeRoomChannel();
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        isSubscribed = true;
        AddChatText("Room chat joined.");
        StartCoroutine(ScrollToBottomRoutine());
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        if (channelName != currentChannelName)
            return;

        for (int i = 0; i < messages.Length; i++)
        {
            string rawMessage = messages[i].ToString();
            string[] split = rawMessage.Split('|');

            if (split.Length >= 2)
            {
                AddChatText($"{split[0]} : {split[1]}");
            }
            else
            {
                AddChatText($"{senders[i]} : {messages[i]}");
            }
        }

        StartCoroutine(ScrollToBottomRoutine());
    }

    public void OnUnsubscribed(string[] channels)
    {
        isSubscribed = false;
    }

    public void OnDisconnected() { }
    public void OnChatStateChange(ChatState state) { }
    public void DebugReturn(LogLevel level, string message) { }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }
    public void OnPrivateMessage(string sender, object message, string channelName) { }
    public void OnUserSubscribed(string channel, string user) { }
    public void OnUserUnsubscribed(string channel, string user) { }
    public void OnChannelPropertiesChanged(string channel, string senderUserId, System.Collections.Generic.Dictionary<object, object> properties) { }
    public void OnUserPropertiesChanged(string channel, string targetUserId, string senderUserId, System.Collections.Generic.Dictionary<object, object> properties) { }
    public void OnErrorInfo(string channel, string error, object data) { }
    public void OnCustomAuthenticationResponse(System.Collections.Generic.Dictionary<string, object> data) { }
    public void OnCustomAuthenticationFailed(string debugMessage) { }
    public void OnReceiveBroadcastMessage(string channelName, byte[] message) { }
}
