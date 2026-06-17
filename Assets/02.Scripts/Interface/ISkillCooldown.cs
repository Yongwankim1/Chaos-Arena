using Fusion;

public interface ISkillCooldown
{
    TickTimer CooldownTimer { get; }
    float CooldownDuration { get; }
}