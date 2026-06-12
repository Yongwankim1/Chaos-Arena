using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomActionButtonUI : MonoBehaviour
{
    [SerializeField] private Button roomActionButton;
    [SerializeField] private TMP_Text roomActionButtonText;
    [SerializeField] private int gameSceneBuildIndex = 1;
    [SerializeField] private bool allowSinglePlay;

    private NetworkRunner runner;

    private void OnEnable()
    {
        roomActionButton.onClick.AddListener(OnClickRoomActionButton);
        RoomPlayerData.OnRoomPlayerDataChanged += Refresh;
    }

    private void OnDisable()
    {
        roomActionButton.onClick.RemoveListener(OnClickRoomActionButton);
        RoomPlayerData.OnRoomPlayerDataChanged -= Refresh;
    }

    public void Init(NetworkRunner newRunner)
    {
        runner = newRunner;
        Refresh();
    }

    public void Clear()
    {
        runner = null;
        roomActionButtonText.text = "";
        roomActionButton.interactable = false;
    }

    public void Refresh()
    {
        if (runner == null)
        {
            roomActionButton.interactable = false;
            return;
        }

        if (runner.IsServer)
        {
            roomActionButtonText.text = "시작";
            roomActionButton.interactable = CanStartGame();
        }
        else
        {
            RoomPlayerData localData = GetLocalRoomPlayerData();

            bool isReady = false;

            if (localData != null)
            {
                try
                {
                    isReady = localData.IsReady;
                }
                catch
                {
                    isReady = false;
                }
            }

            roomActionButtonText.text = isReady ? "레디 취소" : "레디";
            roomActionButton.interactable = true;
        }
    }

    private void OnClickRoomActionButton()
    {
        if (runner == null) return;

        if (runner.IsServer)
        {
            TryStartGame();
        }
        else
        {
            ToggleReady();
        }
    }

    private void ToggleReady()
    {
        RoomPlayerData localData = GetLocalRoomPlayerData();

        if (localData == null)
        {
            Debug.Log("내 RoomPlayerData를 찾지 못했습니다.");
            return;
        }

        bool isReady;

        try
        {
            isReady = localData.IsReady;
        }
        catch
        {
            return;
        }

        localData.RPC_SetReady(!isReady);
    }

    private void TryStartGame()
    {
        if (runner == null) return;
        if (!runner.IsServer) return;
        if (!CanStartGame()) return;

        Debug.Log("게임 시작");

        RoomSessionData.RoomName = runner.SessionInfo.Name;

        RoomSessionData.IsHost = runner.IsServer;

        runner.SessionInfo.IsOpen = false;
        runner.SessionInfo.IsVisible = false;
        runner.SessionInfo.UpdateCustomProperties(new Dictionary<string, SessionProperty>
        {
            { "isPlaying", true }
        });
        runner.LoadScene(SceneRef.FromIndex(gameSceneBuildIndex));
    }

    private bool CanStartGame()
    {
        if (allowSinglePlay && HasAnyValidPlayer())
            return true;

        return AreAllClientsReady();
    }

    private bool HasAnyValidPlayer()
    {
        RoomPlayerData[] players =
            FindObjectsByType<RoomPlayerData>(FindObjectsSortMode.None);

        foreach (RoomPlayerData player in players)
        {
            if (player == null) continue;
            if (player.Object == null) continue;
            if (!player.Object.IsValid) continue;

            return true;
        }

        return false;
    }

    private bool AreAllClientsReady()
    {
        RoomPlayerData[] players =
            FindObjectsByType<RoomPlayerData>(FindObjectsSortMode.None);

        if (players.Length < 2)
            return false;

        foreach (RoomPlayerData player in players)
        {
            if (player == null) continue;
            if (player.Object == null) continue;
            if (!player.Object.IsValid) continue;

            if (player.Object.InputAuthority == runner.LocalPlayer)
                continue;

            bool isReady;

            try
            {
                isReady = player.IsReady;
            }
            catch
            {
                return false;
            }

            if (!isReady)
                return false;
        }

        return true;
    }

    private RoomPlayerData GetLocalRoomPlayerData()
    {
        RoomPlayerData[] players =
            FindObjectsByType<RoomPlayerData>(FindObjectsSortMode.None);

        foreach (RoomPlayerData player in players)
        {
            if (player == null) continue;
            if (player.Object == null) continue;
            if (!player.Object.IsValid) continue;

            if (player.Object.HasInputAuthority)
                return player;
        }

        return null;
    }
}
