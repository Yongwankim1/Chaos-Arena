using Fusion;
using UnityEngine;

public class AssassinStealth : NetworkBehaviour, IStealthHandler, ISkillE
{
    [SerializeField] private float duration = 5f;
    [SerializeField] private float cooldown = 15f;
    [SerializeField]
    private float manaCost = 30f;

    [SerializeField] private ParticleSystem stealthStartEffect;
    [SerializeField] private ParticleSystem stealthEndEffect;

    [Networked] public NetworkBool IsStealth { get; set; }
    [Networked] public TickTimer StealthTimer { get; set; }
    [Networked] public TickTimer CooldownTimer { get; set; }

    public TickTimer Cooldown => CooldownTimer;
    public bool IsStealthed => IsStealth;
    public float CooldownDuration => cooldown;

    private SkinnedMeshRenderer[] _renderers;
    private Material[] _originalMaterials;

    private PlayerCharacter _player;
    private PlayerVisualController _visual;
    private AssassinUltimate _ultimate;

    private void Awake()
    {
        _player = GetComponent<PlayerCharacter>();

        _visual = GetComponent<PlayerVisualController>();

        _renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        _ultimate = GetComponent<AssassinUltimate>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (!IsStealth)
            return;

        if (StealthTimer.Expired(Runner))
        {
            ExitStealth();
        }
    }

    public void UseE()
    {
        if (!HasStateAuthority)
            return;

        if (_player.IsDead)
            return;

        if (IsStealth)
            return;

        if (!CooldownTimer.ExpiredOrNotRunning(Runner))
            return;
        if (!_player.UseMana(manaCost))
            return;
        IsStealth = true;

        StealthTimer = TickTimer.CreateFromSeconds(Runner, duration);

        CooldownTimer = TickTimer.CreateFromSeconds(Runner, cooldown);

        RPC_OnStealthStart();
    }

    public void ExitStealth()
    {
        if (!HasStateAuthority)
            return;

        if (!IsStealth)
            return;

        IsStealth = false;

        RPC_OnStealthEnd();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OnStealthStart()
    {
        stealthStartEffect?.Play();

        _visual?.SetStealth(true);

        if (!HasInputAuthority)
        {
            foreach (var renderer in _renderers)
            {
                renderer.enabled = false;
            }
        }
        _ultimate?.RefreshUltimateEffectVisibility();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OnStealthEnd()
    {
        stealthEndEffect?.Play();

        foreach (var renderer in _renderers)
        {
            renderer.enabled = true;
        }

        _visual?.SetStealth(false);
        _ultimate?.RefreshUltimateEffectVisibility();
    }
}