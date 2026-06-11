using UnityEngine;

public class EnemyBuff : Buff, IBuffable
{
    [SerializeField] bool isBuff;
    [SerializeField] GameObject effect;
    public bool IsBuff => isBuff;
    public void AddBuff()
    {
        isBuff = true;
        effect = Instantiate(buff.Effect, transform.position, Quaternion.identity, transform);

    }

    public BuffSO GetBuff()
    {
        return buff;
    }

    public void RemoveBuff()
    {
        isBuff = false;
        Destroy(effect);
    }
}
