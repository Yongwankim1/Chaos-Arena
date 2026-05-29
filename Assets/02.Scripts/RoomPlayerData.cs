using Fusion;

public class RoomPlayerData : NetworkBehaviour
{
    [Networked]
    public NetworkString<_32> NickName { get; set; }

    [Networked]
    public NetworkBool IsReady { get; set; }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            RPC_SetNickName(UserDataManager.Instance.UserData.UserName);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetNickName(string nickName)
    {
        NickName = nickName;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetReady(bool isReady)
    {
        IsReady = isReady;
    }
}