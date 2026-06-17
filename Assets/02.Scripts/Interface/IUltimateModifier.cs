using UnityEngine;

public interface IUltimateModifier
{
    bool IsUltimateActive { get; }

    float GetAttackMultiplier(int comboIndex);
    GameObject GetOverrideEffect(int comboIndex);
}