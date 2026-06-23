using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class CharacterActionLock : NetworkBehaviour
{
    private readonly Dictionary<ActionLockType, int> _lockCounts = new();

    public bool CanMove => !_lockCounts.ContainsKey(ActionLockType.Move);
    public bool CanAttack => !_lockCounts.ContainsKey(ActionLockType.Attack);
    public bool CanDash => !_lockCounts.ContainsKey(ActionLockType.Dash);
    public bool CanJump => !_lockCounts.ContainsKey(ActionLockType.Jump);
    public bool CanSkill => !_lockCounts.ContainsKey(ActionLockType.Skill);

    public void Lock(ActionLockType type)
    {
        if (!HasStateAuthority)
            return;

        AddLock(type);
        RPC_AddLock(type);
    }

    public void Unlock(ActionLockType type)
    {
        if (!HasStateAuthority)
            return;

        RemoveLock(type);
        RPC_RemoveLock(type);
    }

    public void ClearAll()
    {
        if (!HasStateAuthority)
            return;

        _lockCounts.Clear();
        RPC_ClearAll();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AddLock(ActionLockType type)
    {
        if (HasStateAuthority)
            return;

        AddLock(type);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_RemoveLock(ActionLockType type)
    {
        if (HasStateAuthority)
            return;

        RemoveLock(type);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ClearAll()
    {
        if (HasStateAuthority)
            return;

        _lockCounts.Clear();
    }

    private void AddLock(ActionLockType type)
    {
        if (!_lockCounts.ContainsKey(type))
            _lockCounts[type] = 0;

        _lockCounts[type]++;
    }

    private void RemoveLock(ActionLockType type)
    {
        if (!_lockCounts.ContainsKey(type))
            return;

        _lockCounts[type]--;

        if (_lockCounts[type] <= 0)
            _lockCounts.Remove(type);
    }
}
