public interface IActiveSkill
{
    bool IsActive { get; }
    float RemainingDuration { get; }
    float Duration { get; }
}