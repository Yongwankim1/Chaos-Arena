using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance { get; private set; }

    [SerializeField] private List<BuffSO> buffList;

    private Dictionary<BuffType, BuffSO> buffMap;

    private void Awake()
    {
        Instance = this;

        buffMap = new Dictionary<BuffType, BuffSO>();

        foreach (BuffSO buff in buffList)
        {
            buffMap[buff.Type] = buff;
        }
    }

    public BuffSO GetBuff(BuffType type)
    {
        buffMap.TryGetValue(type, out BuffSO buff);
        return buff;
    }
}