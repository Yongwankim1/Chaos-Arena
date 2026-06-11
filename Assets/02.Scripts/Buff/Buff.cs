using UnityEngine;

public class Buff : MonoBehaviour, IBuffable
{
    [SerializeField] BuffSO buff;
    public void AddBuff()
    {

    }

    public BuffSO GetBuff()
    {
        return buff;
    }

    public void RemoveBuff()
    {

    }
}
