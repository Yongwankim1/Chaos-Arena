using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssassinUltimate : NetworkBehaviour, IUltimateModifier , ISkillR
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

    [SerializeField]
    private GameObject combo123ShadowEffect;

    [SerializeField]
    private GameObject combo4ShadowEffect;

    [SerializeField]
    private GameObject combo5ShadowEffect;

    [SerializeField]
    private float shadowDelay = 0.3f;

    [SerializeField]
    private float shadowDamageMultiplier = 0.3f;

    [Header("Q Upgrade")]
    [SerializeField]
    private NetworkObject shadowShurikenPrefab;

    [SerializeField]
    private GameObject shadowAttackEffect;

    private CharacterCombat _combat;
    private PlayerCharacter _player;
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
    public bool IsUltimateActive =>
    IsUltimate;

    public float GetAttackMultiplier(
        int comboIndex)
    {
        if (!IsUltimate)
            return 1f;

        if (comboIndex == 4 ||
            comboIndex == 5)
        {
            return 1.2f;
        }

        return 1f;
    }

    [Networked]
    public NetworkBool IsUltimate { get; set; }

    [Networked]
    private TickTimer UltimateTimer { get; set; }

    [Networked]
    public TickTimer CooldownTimer { get; set; }

    private SkinnedMeshRenderer[] _renderers;

    private Material[] _originMaterials;

    public float CooldownDuration => cooldown;

    public NetworkObject ShadowShurikenPrefab => shadowShurikenPrefab;

    private void Awake()
    {
        _player =GetComponent<PlayerCharacter>();
        _renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        _combat = GetComponent<CharacterCombat>();
        _originMaterials =
            new Material[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            _originMaterials[i] =
                _renderers[i].material;
        }
    }
    public override void Spawned()
    {
        if (_combat != null)
        {
            _combat.OnAttackSpawned += OnAttack;
        }
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (_combat != null)
        {
            _combat.OnAttackSpawned -= OnAttack;
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
    private void OnAttack(
    int comboIndex,
    AttackData data)
    {
        if (!HasStateAuthority)
            return;

        if (!IsUltimate)
            return;

        if (comboIndex == 4)
        {
            SpawnDashShadowEffect(
                combo4ShadowEffect);

            return;
        }

        if (comboIndex == 5)
        {
            SpawnDashShadowEffect(
                combo5ShadowEffect);

            return;
        }

        StartCoroutine(
            ShadowAttackRoutine(data));
    }
    private IEnumerator ShadowAttackRoutine(
     AttackData data)
    {
        yield return new WaitForSeconds(
            shadowDelay);

        if (!IsUltimate)
            yield break;

        Vector3 spawnPosition =
            _combat.AttackSpawnPoint.position;

        if (combo123ShadowEffect != null)
        {
            Instantiate(
                combo123ShadowEffect,
                spawnPosition,
                transform.rotation);
        }

        PerformShadowHitBox(
            data,
            shadowDamageMultiplier);
    }

    private void SpawnDashShadowEffect(
    GameObject effectPrefab)
    {
        if (effectPrefab == null)
            return;

        Instantiate(
            effectPrefab,
            transform.position,
            transform.rotation);
    }

    private void PerformShadowHitBox(
    AttackData data,
    float damageMultiplier)
    {
        HashSet<IDamageable> damagedTargets =
            new HashSet<IDamageable>();

        Vector3 center =
             _combat.AttackSpawnPoint.position +
             _combat.AttackSpawnPoint.forward *
            (data.Range * 0.5f);

        Collider[] hits =
            Physics.OverlapBox(
                center,
                new Vector3(
                    data.Radius,
                    1f,
                    data.Range * 0.5f),
                 _combat.AttackSpawnPoint.rotation);

        foreach (Collider hit in hits)
        {
            if (hit.transform.root ==
                transform.root)
                continue;

            IDamageable damageable =
                hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            if (damagedTargets.Contains(
                damageable))
                continue;

            damagedTargets.Add(
                damageable);

            float damage =
    _player.AttackPower *
    (data.DamagePercent / 100f) *
    damageMultiplier;

            damageable.TakeDamage(
     Mathf.RoundToInt(damage),
     _combat);
        }
    }
}