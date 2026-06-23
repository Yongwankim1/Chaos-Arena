using Fusion;
using System.Collections.Generic;
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

        List<RoomPlayerData> playerList = new List<RoomPlayerData>();

        foreach (RoomPlayerData player in players)
        {
            if (player == null)
                continue;

            if (player.Object == null)
                continue;

            if (!player.Object.IsValid)
                continue;

            playerList.Add(player);
        }

        RoomPlayerData localPlayer =
            playerList.Find(x =>
                x != null &&
                x.Object != null &&
                x.Object.HasInputAuthority);

        if (localPlayer != null)
        {
            playerList.Remove(localPlayer);

            int centerIndex = Mathf.Clamp( playerList.Count / 2, 0, playerList.Count);

            playerList.Insert(centerIndex, localPlayer);
        }

        foreach (RoomPlayerData player in playerList)
        {
            UserInfo userInfo = Instantiate( userInfoPrefab, userInfoParent);

            userInfo.Init(player);
        }

        UpdateContentPosition(playerList.Count);
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

    private void UpdateContentPosition(int playerCount)
    {
        RectTransform rect = userInfoParent as RectTransform;

        if (rect == null)
            return;

        Vector2 pos = rect.anchoredPosition;

        pos.x = playerCount == 6 ? -130f : 0f;

        rect.anchoredPosition = pos;
    }
}
