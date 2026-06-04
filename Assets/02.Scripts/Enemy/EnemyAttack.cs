using Fusion;
using UnityEngine;

public class EnemyAttack : NetworkBehaviour, IAttacker
{
    [SerializeField] NetworkMecanimAnimator netAnimator;
    [SerializeField] private string attackParam = "Attack";
    private void Awake()
    {
        if (netAnimator == null)
        {
            netAnimator = GetComponent<NetworkMecanimAnimator>();
        }
    }

    public void Attack()
    {
        if (Object != null && !Object.HasStateAuthority)
            return;
        if (netAnimator == null) return;

        netAnimator.SetTrigger(attackParam, true);
        Debug.Log("Attack animation started");
    }

    public void OnEffect()
    {
        if (Object != null && !Object.HasStateAuthority)
            return;

        RPC_PlayEffect();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayEffect()
    {
        // TODO: Play attack effect and sound.
        Debug.Log("Attack effect");
    }

    public GameObject GetAttacker()
    {
        return gameObject;
    }
}
