using Fusion;
using System;

public class RoomPlayerData : NetworkBehaviour
{
    public static event Action OnRoomPlayerDataChanged;

    [Networked] public NetworkString<_32> NickName { get; set; }
    [Networked] public NetworkBool IsReady { get; set; }

    public override void Spawned()
    {
        if (!Object.HasInputAuthority)
            return;

        string nickName = UserDataManager.Instance.UserData.UserName;

        if (string.IsNullOrWhiteSpace(nickName))
        {
            nickName = "Player";
        }

        RPC_SetNickName(nickName);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        OnRoomPlayerDataChanged?.Invoke();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetNickName(string nickName)
    {
        NickName = nickName;
        RPC_NotifyRoomPlayerDataChanged();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetReady(bool isReady)
    {
        IsReady = isReady;
        RPC_NotifyRoomPlayerDataChanged();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_RequestExitRoomByHost()
    {
        if (!Object.HasInputAuthority)
            return;

        LobbyManager lobby = FindFirstObjectByType<LobbyManager>();
        if (lobby == null)
            return;

        lobby.ExitRoomRequestedByHost();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NotifyRoomPlayerDataChanged()
    {
        OnRoomPlayerDataChanged?.Invoke();
    }
}
