using Fusion;
using UnityEngine;

public class AssassinUltimate : NetworkBehaviour
{
    [Header("Setting")]
    [SerializeField]
    private float duration = 12f;

    [SerializeField]
    private float cooldown = 10f;

    [SerializeField]
    private float manaCost = 60f;

    [Header("Material")]
    [SerializeField]
    private Material shadowMaterial;

    [Header("Effect")]
    [SerializeField]
    private ParticleSystem startEffect;

    [SerializeField]
    private ParticleSystem endEffect;

    [SerializeField]
    private ParticleSystem shadowSmoke;

    [Header("Q Upgrade")]
    [SerializeField]
    private NetworkObject shadowShurikenPrefab;

    public float Duration => duration;
    public float RemainingDuration
    {
        get
        {
            if (!IsUltimate)
                return 0f;

            return UltimateTimer.RemainingTime(
                Runner) ?? 0f;
        }
    }

    [Networked]
    public NetworkBool IsUltimate { get; set; }

    [Networked]
    private TickTimer UltimateTimer { get; set; }

    [Networked]
    public TickTimer CooldownTimer { get; set; }

    private PlayerCharacter _player;

    private SkinnedMeshRenderer[] _renderers;

    private Material[] _originMaterials;

    public float CooldownDuration => cooldown;

    public NetworkObject ShadowShurikenPrefab => shadowShurikenPrefab;

    private void Awake()
    {
        _player =
            GetComponent<PlayerCharacter>();

        _renderers =
            GetComponentsInChildren<SkinnedMeshRenderer>(true);

        _originMaterials =
            new Material[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            _originMaterials[i] =
                _renderers[i].material;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (!IsUltimate)
            return;

        if (UltimateTimer.Expired(Runner))
        {
            EndUltimate();
        }
    }

    public void UseR()
    {
        if (!HasStateAuthority)
            return;

        if (_player.IsDead)
            return;

        if (IsUltimate)
            return;

        if (!CooldownTimer.ExpiredOrNotRunning(Runner))
            return;

        if (!_player.UseMana(manaCost))
            return;

        IsUltimate = true;

        UltimateTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                duration);

        RPC_StartUltimate();
    }

    private void EndUltimate()
    {
        if (!IsUltimate)
            return;

        IsUltimate = false;

        UltimateTimer = TickTimer.None;

        CooldownTimer = TickTimer.CreateFromSeconds(Runner, cooldown);

        RPC_EndUltimate();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_StartUltimate()
    {
        startEffect?.Play();

        shadowSmoke?.Play();

        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].material = shadowMaterial;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_EndUltimate()
    {
        endEffect?.Play();

        shadowSmoke?.Stop();

        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].material = _originMaterials[i];
        }
    }
}