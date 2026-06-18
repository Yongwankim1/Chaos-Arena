using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssassinUltimate : NetworkBehaviour, IUltimateModifier, ISkillR, ISkillCooldown, IActiveSkill
{
    [Header("Setting")]
    [SerializeField]
    private float duration = 12f;

    [SerializeField]
    private float cooldown = 10f;

    [SerializeField]
    private float manaCost = 60f;

    public bool IsActive => IsUltimate;

    [Header("Effect")]
    [SerializeField]
    private ParticleSystem startEffect;

    [SerializeField]
    private ParticleSystem endEffect;

    [SerializeField]
    private ParticleSystem shadowSmoke;

    [SerializeField]
    private GameObject combo1ShadowEffect;

    [SerializeField]
    private GameObject combo23ShadowEffect;

    [SerializeField]
    private GameObject combo4ShadowEffect;

    [SerializeField]
    private GameObject combo5ShadowEffect;

    [SerializeField]
    private float shadowDelay = 0.3f;

    [SerializeField]
    private float shadowDamageMultiplier = 0.5f;
    private ParticleSystemRenderer[] _smokeRenderers;

    [Header("Q Upgrade")]
    [SerializeField]
    private NetworkObject shadowShurikenPrefab;

    [SerializeField]
    private GameObject shadowAttackEffect;

    private CharacterCombat _combat;
    private PlayerCharacter _player;
    private PlayerVisualController _visual;
    private AssassinStealth _stealth;
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
    public bool IsUltimateActive => IsUltimate;

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
    public GameObject GetOverrideEffect(int comboIndex)
    {
        if (!IsUltimate)
            return null;

        switch (comboIndex)
        {
            case 4:
                return combo4ShadowEffect;

            case 5:
                return combo5ShadowEffect;
        }

        return null;
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
        _player = GetComponent<PlayerCharacter>();
        _combat = GetComponent<CharacterCombat>();
        _visual = GetComponent<PlayerVisualController>();

        _stealth = GetComponent<AssassinStealth>();

        if (shadowSmoke != null)
        {
            _smokeRenderers = shadowSmoke.GetComponentsInChildren<ParticleSystemRenderer>(true);
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
    private bool CanSeeStealthEffect()
    {
        return _stealth == null
            || !_stealth.IsStealth
            || Object.HasInputAuthority;
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
        bool visible =
            !(_stealth != null &&
              _stealth.IsStealth &&
              !HasInputAuthority);

        if (visible)
        {
            startEffect?.Play();
        }

        shadowSmoke?.Play();

        RefreshUltimateEffectVisibility();

        _visual?.SetUltimate(true);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_EndUltimate()
    {
        bool visible =
            !(_stealth != null &&
              _stealth.IsStealth &&
              !HasInputAuthority);

        if (visible)
        {
            endEffect?.Play();
        }

        shadowSmoke?.Stop();

        _visual?.SetUltimate(false);
    }
    private void OnAttack(
    int comboIndex,
    AttackData data)
    {
        if (!HasStateAuthority)
            return;

        if (!IsUltimate)
            return;

        switch (comboIndex)
        {
            case 1:
                StartCoroutine(
     ShadowAttackRoutine(
         comboIndex,
         data,
         combo1ShadowEffect));
                break;

            case 2:
            case 3:
                StartCoroutine(
    ShadowAttackRoutine(
        comboIndex,
        data,
        combo23ShadowEffect));
                break;

            case 4:

                break;

            case 5:

                break;
        }
    }
    private IEnumerator ShadowAttackRoutine(
     int comboIndex,
     AttackData data,
     GameObject effectPrefab)
    {
        yield return new WaitForSeconds(
            shadowDelay);

        if (!IsUltimate)
            yield break;

        Vector3 spawnPosition =
            _combat.AttackSpawnPoint.position +
            transform.TransformDirection(
                data.EffectPositionOffset);

        Quaternion spawnRotation =
            transform.rotation *
            Quaternion.Euler(
                data.EffectRotationOffset);

        if (effectPrefab != null)
        {
            RPC_SpawnShadowEffect(
           comboIndex,
           spawnPosition,
           spawnRotation);
        }

        PerformShadowHitBox(
            data,
            shadowDamageMultiplier);
    }

    private void SpawnDashShadowEffect(
     GameObject effectPrefab,
     AttackData data)
    {
        if (effectPrefab == null)
            return;

        Vector3 spawnPosition =
            _combat.AttackSpawnPoint.position +
            transform.TransformDirection(
                data.EffectPositionOffset);

        Quaternion spawnRotation =
            transform.rotation *
            Quaternion.Euler(
                data.EffectRotationOffset);

        Instantiate(
            effectPrefab,
            spawnPosition,
            spawnRotation);
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
            HitFeedbackSystem.Apply(_combat, damageable, data);
        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SpawnShadowEffect(
    int comboIndex,
    Vector3 position,
    Quaternion rotation)
    {
        GameObject prefab = null;

        switch (comboIndex)
        {
            case 1:
                prefab = combo1ShadowEffect;
                break;

            case 2:
            case 3:
                prefab = combo23ShadowEffect;
                break;
        }

        if (prefab == null)
            return;

        if (!CanSeeStealthEffect())
            return;

        GameObject effect =
            Instantiate(
                prefab,
                position,
                rotation);

    }
    public void RefreshUltimateEffectVisibility()
    {
        bool visible =
            !(_stealth != null &&
              _stealth.IsStealth &&
              !HasInputAuthority);

        if (_smokeRenderers != null)
        {
            foreach (var renderer in _smokeRenderers)
            {
                renderer.enabled = visible && IsUltimate;
            }
        }
    }
}