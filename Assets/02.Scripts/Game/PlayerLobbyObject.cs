using Fusion;
using UnityEngine;

public class PlayerLobbyObject : NetworkBehaviour
{
    [Networked]
    public CharacterClassType SelectedClass { get; set; }

    [Networked]
    public NetworkBool IsReady { get; set; }

    public static PlayerLobbyObject Local;

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            Local = this;
        }
    }

    [Rpc(
      RpcSources.InputAuthority,
      RpcTargets.StateAuthority)]
    public void RPC_SelectCharacter(
      CharacterClassType classType)
    {
        SelectedClass = classType;

        Debug.Log(
            $"Player {Object.InputAuthority.PlayerId} selected {classType}");

        GameBootstrap bootstrap =
            FindFirstObjectByType<GameBootstrap>();

        bootstrap.SpawnSelectedCharacter(
            Object.InputAuthority);
    }

    [Rpc(
        RpcSources.InputAuthority,
        RpcTargets.StateAuthority)]
    public void RPC_SetReady()
    {
        IsReady = true;
    }
}