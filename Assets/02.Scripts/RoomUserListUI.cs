using UnityEngine;

public class RoomUserListUI : MonoBehaviour
{
    [SerializeField] private Transform userInfoParent;
    [SerializeField] private UserInfo userInfoPrefab;

    private void OnEnable()
    {
        RoomPlayerData.OnRoomPlayerDataChanged += Refresh;
    }

    private void OnDisable()
    {
        RoomPlayerData.OnRoomPlayerDataChanged -= Refresh;
    }

    public void Refresh()
    {
        Clear();

        RoomPlayerData[] players =
            FindObjectsByType<RoomPlayerData>(FindObjectsSortMode.None);

        foreach (RoomPlayerData player in players)
        {
            if (!TryReadPlayerData(player, out string nickName, out bool isReady))
                continue;

            UserInfo userInfo = Instantiate(userInfoPrefab, userInfoParent);
            userInfo.Init(nickName, isReady);
        }
    }

    public void Clear()
    {
        foreach (Transform child in userInfoParent)
        {
            Destroy(child.gameObject);
        }
    }

    private bool TryReadPlayerData(RoomPlayerData player, out string nickName, out bool isReady)
    {
        nickName = "";
        isReady = false;

        if (player == null) return false;
        if (player.Object == null) return false;
        if (!player.Object.IsValid) return false;

        try
        {
            nickName = player.NickName.ToString();
            isReady = player.IsReady;
        }
        catch
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(nickName))
        {
            nickName = "Player";
        }

        return true;
    }
}