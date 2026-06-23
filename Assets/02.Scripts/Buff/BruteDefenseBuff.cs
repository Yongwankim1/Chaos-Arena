using Fusion;
using static Unity.Collections.Unicode;

public class BruteDefenseBuff : NetworkBehaviour
{
    [Networked]
    public TickTimer BuffTimer { get; set; }

    [Networked]
    public NetworkBool BuffActive { get; set; }

    private PlayerCharacter _player;

    private BruteBuffEffect _effect;

    private void Awake()
    {
        _player = GetComponent<PlayerCharacter>();

        _effect = GetComponent<BruteBuffEffect>();
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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetEffect(bool visible)
    {
        _effect?.SetVisible(visible);
    }
}