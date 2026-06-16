using Fusion;
using UnityEngine;

public class AssassinStealth : NetworkBehaviour
{
    [SerializeField] private float duration = 5f;
    [SerializeField] private float cooldown = 15f;

    [SerializeField] private Material stealthMaterial;

    [SerializeField] private ParticleSystem stealthStartEffect;
    [SerializeField] private ParticleSystem stealthEndEffect;

    [Networked] public NetworkBool IsStealth { get; set; }
    [Networked] public TickTimer StealthTimer { get; set; }
    [Networked] public TickTimer CooldownTimer { get; set; }

    private SkinnedMeshRenderer[] _renderers;
    private Material[] _originalMaterials;

    private PlayerCharacter _player;

    private void Awake()
    {
        _player = GetComponent<PlayerCharacter>();

        _renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);

        _originalMaterials = new Material[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalMaterials[i] = _renderers[i].material;
        }
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

        if (HasInputAuthority)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].material = stealthMaterial;
            }
        }
        else
        {
            foreach (var renderer in _renderers)
            {
                renderer.enabled = false;
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OnStealthEnd()
    {
        stealthEndEffect?.Play();

        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].enabled = true;
            _renderers[i].material = _originalMaterials[i];
        }
    }
}