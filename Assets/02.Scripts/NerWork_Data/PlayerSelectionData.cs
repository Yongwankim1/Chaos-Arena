using Fusion;
using UnityEngine;

public class PlayerSelectionData : NetworkBehaviour
{
    [Networked]
    public CharacterClassType SelectedClass { get; set; }

    [Networked]
    public NetworkBool IsReady { get; set; }

    [Rpc(RpcSources.InputAuthority,RpcTargets.StateAuthority)]
    public void RPC_SelectCharacter(CharacterClassType classType)
    {
        TeamType myTeam =GameBootstrap.Instance.GetPlayerTeam(Object.InputAuthority);

        if (GameBootstrap.Instance.IsCharacterUsedInTeam(classType,myTeam,Object.InputAuthority))
        {
            return;
        }

        SelectedClass = classType;

        Debug.Log($"Player {Object.InputAuthority.PlayerId} selected {classType}");

        GameBootstrap bootstrap =
            FindFirstObjectByType<GameBootstrap>();

        bootstrap.SpawnSelectedCharacter(
            Object.InputAuthority);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetReady()
    {
        IsReady = true;
    }
}