using Fusion;
using UnityEngine;

public class PlayerLobbyObject : NetworkBehaviour
{
    [Networked]
    public CharacterClassType SelectedClass { get; set; }

    [Networked]
    public NetworkBool IsReady { get; set; }

    public static PlayerLobbyObject Local;
    public static event System.Action OnCharacterSelectionChanged;
    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            Local = this;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SelectCharacter(CharacterClassType classType)
    {
        TeamType myTeam =GameBootstrap.Instance.GetPlayerTeam(Object.InputAuthority);

        if (GameBootstrap.Instance.IsCharacterUsedInTeam(
                classType,
                myTeam,
                Object.InputAuthority))
        {
            return;
        }

        SelectedClass = classType;
        RPC_NotifyCharacterSelectionChanged();

        Debug.Log(
            $"Player {Object.InputAuthority.PlayerId} selected {classType}");

        GameBootstrap bootstrap =FindFirstObjectByType<GameBootstrap>();

        bootstrap.SpawnSelectedCharacter(Object.InputAuthority);
    }

    [Rpc(RpcSources.InputAuthority,RpcTargets.StateAuthority)]
    public void RPC_SetReady()
    {
        IsReady = true;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NotifyCharacterSelectionChanged()
    {
        OnCharacterSelectionChanged?.Invoke();
    }
}