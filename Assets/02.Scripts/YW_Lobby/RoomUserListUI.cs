using Fusion;
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
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
            return;

        Clear();

        RoomPlayerData[] players = FindObjectsByType<RoomPlayerData>(FindObjectsSortMode.None);

        foreach (RoomPlayerData player in players)
        {
            if (player == null)
            {
                continue;
            }

            if (player.Object == null)
            {
                continue;
            }

            if (!player.Object.IsValid)
            {
                continue;
            }

            UserInfo userInfo =Instantiate(userInfoPrefab, userInfoParent);

            userInfo.Init(player);
        }
        //foreach (RoomPlayerData player in players)
        //{
        //    if (!TryReadPlayerData(player, out string nickName, out bool isReady))
        //        continue;

        //    UserInfo userInfo = Instantiate(userInfoPrefab, userInfoParent);
        //    userInfo.Init(nickName, isReady);
        //}
    }

    public void Clear()
    {
        for (int i = userInfoParent.childCount - 1; i >= 0; i--)
        {
            Transform child = userInfoParent.GetChild(i);
            child.SetParent(null);
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

    public static int MaxTeamCount
    {
        get
        {
            LobbyManager lobby = FindFirstObjectByType<LobbyManager>();

            if (lobby == null)
            {
                return 1;
            }

            return lobby.GetMaxTeamCount();
        }
    }

    public static int GetBlueCount()
    {

        int count = 0;

        RoomPlayerData[] players = FindObjectsByType<RoomPlayerData>(FindObjectsSortMode.None);

        foreach (RoomPlayerData player in players)
        {
            if (player == null)
            {
                continue;
            }

            if (player.Object == null)
            {
                continue;
            }

            if (!player.Object.IsValid)
            {
                continue;
            }

            if (player.TeamSelect == TeamSelectType.Blue)
            {
                count++;
            }
        }

        return count;
    }

    public static int GetRedCount()
    {
        int count = 0;

        RoomPlayerData[] players = FindObjectsByType<RoomPlayerData>(FindObjectsSortMode.None);

        foreach (RoomPlayerData player in players)
        {
            if (player == null)
            {
                continue;
            }

            if (player.Object == null)
            {
                continue;
            }

            if (!player.Object.IsValid)
            {
                continue;
            }

            if (player.TeamSelect == TeamSelectType.Red)
            {
                count++;
            }
        }

        return count;
    }
}
