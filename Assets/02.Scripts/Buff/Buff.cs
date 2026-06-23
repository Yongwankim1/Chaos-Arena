using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class Buff : NetworkBehaviour
{
    [SerializeField] private List<BuffSO> buffs = new List<BuffSO>();
    [SerializeField] private List<GameObject> effects = new List<GameObject>();
    [SerializeField] private Transform effectRoot;

    public void Init()
    {
        if (Object != null && !Object.HasStateAuthority)
            return;

        RPC_Init();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_Init()
    {
        foreach (BuffSO buff in buffs)
        {
            RemoveBuff(buff);
        }

        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i] == null)
                continue;

            Destroy(effects[i]);
            effects[i] = null;
        }

        buffs.Clear();
        effects.Clear();
    }
    public void AddBuff(BuffType type)
    {
        if (Object == null || !Object.HasStateAuthority)
            return;

        BuffSO buff = BuffManager.Instance.GetBuff(type);
        if (buff == null)
            return;

        if (buffs.Contains(buff))
            return;

        buffs.Add(buff);

        ApplyBuff(buff);
        RPC_AddBuffEffect(type);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AddBuffEffect(BuffType type)
    {
        BuffSO buff = BuffManager.Instance.GetBuff(type);
        if (buff == null || buff.Effect == null)
            return;

        Transform parent = effectRoot != null ? effectRoot : transform;
        GameObject effect = Instantiate(buff.Effect, parent);
        effect.transform.localPosition = Vector3.zero;
        effect.transform.localRotation = Quaternion.identity;

        effects.Add(effect);
    }

    private void ApplyBuff(BuffSO buff)
    {
        // TODO: Apply real stat changes here, such as attack or movement speed.
        switch (buff.Type)
        {
            case BuffType.None: break;
            case BuffType.Red:
                IRedBuffable redBuff = GetComponent<IRedBuffable>();
                if (redBuff == null) return;
                redBuff.OnRedBuff(buff,true);
                break;
            case BuffType.Blue:
                IBlueBuffable blueBuff = GetComponent<IBlueBuffable>();
                if (blueBuff == null) return;
                blueBuff.OnBlueBuff(buff,true);
                break;
        }

    }
    private void RemoveBuff(BuffSO buff)
    {
        switch (buff.Type)
        {
            case BuffType.Blue:
                GetComponent<IBlueBuffable>()?.OnBlueBuff(buff, false);
                break;

            case BuffType.Red:
                GetComponent<IRedBuffable>()?.OnRedBuff(buff, false);
                break;
        }
    }
    public void SetEffectVisible(bool visible)
    {
        foreach (GameObject effect in effects)
        {
            if (effect == null)
                continue;

            effect.SetActive(visible);
        }
    }
}
