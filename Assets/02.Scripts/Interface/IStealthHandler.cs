public interface IStealthHandler
{
    bool IsStealthed { get; }
    void ExitStealth();
}