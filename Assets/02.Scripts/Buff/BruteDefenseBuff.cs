using Fusion;

public class BruteDefenseBuff : NetworkBehaviour
{
    [Networked]
    public TickTimer BuffTimer { get; set; }

    [Networked]
    public NetworkBool BuffActive { get; set; }

    private PlayerCharacter _player;

    private BruteBuffEffect _effect;

    private bool _effectActive;

    private bool _hiddenByStealth;

    private void Awake()
    {
        _player = GetComponent<PlayerCharacter>();

        _effect = GetComponent<BruteBuffEffect>();
    }

    public void RequestApply(
        float reduction,
        float duration)
    {
        if (Object != null &&
            !Object.HasStateAuthority)
        {
            RPC_RequestApply(
                reduction,
                duration);

            return;
        }

        Apply(
            reduction,
            duration);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestApply(
        float reduction,
        float duration)
    {
        Apply(
            reduction,
            duration);
    }

    public void Apply(
        float reduction,
        float duration)
    {
        if (!HasStateAuthority)
            return;

        _player.DamageReductionPercent =
            reduction;

        BuffActive = true;

        BuffTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                duration);

        RPC_SetEffect(true);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (!BuffActive)
            return;

        if (!BuffTimer.Expired(Runner))
            return;

        BuffActive = false;

        _player.DamageReductionPercent = 0f;

        RPC_SetEffect(false);
    }

    public void SetStealthHidden(bool hidden)
    {
        _hiddenByStealth = hidden;

        RefreshEffectVisibility();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetEffect(bool visible)
    {
        _effectActive = visible;

        RefreshEffectVisibility();
    }

    private void RefreshEffectVisibility()
    {
        _effect?.SetVisible(_effectActive && !_hiddenByStealth);
    }
}
