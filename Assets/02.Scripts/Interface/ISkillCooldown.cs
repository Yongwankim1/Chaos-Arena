using Fusion;

public interface ISkillCooldown
{
    TickTimer Cooldown { get; }

    float CooldownDuration { get; }
}