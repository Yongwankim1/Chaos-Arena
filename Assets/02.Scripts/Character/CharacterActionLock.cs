using System.Collections.Generic;
using UnityEngine;

public class CharacterActionLock : MonoBehaviour
{
    private Dictionary<ActionLockType, int> _lockCounts = new();

    public bool CanMove => !_lockCounts.ContainsKey(ActionLockType.Move);
    public bool CanAttack => !_lockCounts.ContainsKey(ActionLockType.Attack);
    public bool CanDash => !_lockCounts.ContainsKey(ActionLockType.Dash);

    public void Lock(ActionLockType type)
    {
        if (!_lockCounts.ContainsKey(type))
            _lockCounts[type] = 0;

        _lockCounts[type]++;
    }

    public void Unlock(ActionLockType type)
    {
        if (!_lockCounts.ContainsKey(type))
            return;

        _lockCounts[type]--;

        if (_lockCounts[type] <= 0)
            _lockCounts.Remove(type);
    }

    public void ClearAll()
    {
        _lockCounts.Clear();
    }
}