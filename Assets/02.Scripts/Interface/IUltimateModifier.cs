public interface IUltimateModifier
{
    bool IsUltimateActive { get; }

    float GetAttackMultiplier(int comboIndex);
}