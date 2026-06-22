using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomActionButtonUI : MonoBehaviour
{
    [SerializeField] private Button roomActionButton;
    [SerializeField] private GameObject[] buttonImageUIs = new GameObject[2];
    [SerializeField] private int gameSceneBuildIndex = 1;
    [SerializeField] private bool allowSinglePlay;

    private NetworkRunner runner;
    [SerializeField] private LobbyMapSelector mapSelector;
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
        roomActionButton.interactable = false;
        HideButtonImages();
    }

    public void Refresh()
    {
        if (runner == null)
        {
            roomActionButton.interactable = false;
            HideButtonImages();
            return;
        }

        if (runner.IsServer)
        {
            //roomActionButtonText.text = "시작";
            SetButtonImage(true);
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


            //roomActionButtonText.text = isReady ? "레디 취소" : "레디";
            SetButtonImage(false);
            roomActionButton.interactable = true;
        }
    }
    private void SetButtonImage(bool isHost)
    {
        if (buttonImageUIs == null || buttonImageUIs.Length < 2)
            return;

        if (buttonImageUIs[0] != null)
            buttonImageUIs[0].SetActive(isHost);

        if (buttonImageUIs[1] != null)
            buttonImageUIs[1].SetActive(!isHost);
    }

    private void HideButtonImages()
    {
        if (buttonImageUIs == null)
            return;

        foreach (GameObject buttonImageUI in buttonImageUIs)
        {
            if (buttonImageUI != null)
                buttonImageUI.SetActive(false);
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

        RoomSessionData.TeamSelections.Clear();

        RoomPlayerData[] players = FindObjectsByType<RoomPlayerData>(FindObjectsSortMode.None);

        foreach (RoomPlayerData player in players)
        {
            RoomSessionData.TeamSelections[player.Object.InputAuthority.PlayerId]= player.TeamSelect;
        }

        RoomSessionData.RoomName = runner.SessionInfo.Name;

        RoomSessionData.IsHost = runner.IsServer;

        runner.SessionInfo.IsOpen = false;
        runner.SessionInfo.IsVisible = false;
        runner.SessionInfo.UpdateCustomProperties(new Dictionary<string, SessionProperty>
        {
            { "isPlaying", true }
        });
        runner.LoadScene(SceneRef.FromIndex(mapSelector.GetCreateRoomMapIndex() + 1));
    }

    private bool CanStartGame()
    {
        if (allowSinglePlay && HasAnyValidPlayer())
        {
            return true;
        }

        if (!AreAllClientsReady())
        {
            return false;
        }

        return IsTeamBalanced();
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
    private bool IsTeamBalanced()
    {
        int blueCount = 0;
        int redCount = 0;
        int randomCount = 0;

        RoomPlayerData[] players =FindObjectsByType<RoomPlayerData>(FindObjectsSortMode.None);

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

            switch (player.TeamSelect)
            {
                case TeamSelectType.Blue:
                    blueCount++;
                    break;

                case TeamSelectType.Red:
                    redCount++;
                    break;

                default:
                    randomCount++;
                    break;
            }
        }

        int maxTeamCount =RoomUserListUI.MaxTeamCount;

        int blueNeed =Mathf.Max(0, maxTeamCount - blueCount);

        int redNeed =Mathf.Max(0, maxTeamCount - redCount);

        return randomCount >=blueNeed + redNeed;
    }
}
