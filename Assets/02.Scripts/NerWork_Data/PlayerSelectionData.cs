using Fusion;

public class PlayerSelectionData : NetworkBehaviour
{
    [Networked]
    public CharacterClassType SelectedClass { get; set; }

    [Networked]
    public NetworkBool IsReady { get; set; }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SelectCharacter(
        CharacterClassType classType)
    {
        SelectedClass = classType;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetReady()
    {
        IsReady = true;
    }
}