using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCombat : NetworkBehaviour, IAttacker
{
    [Header("Combo")]
    [SerializeField]
    private float comboInputTime = 0.3f;

    [SerializeField]
    private float attackStateTimeout = 2f;

    private float _attackStateTimer;

    private float _debugAttackEndTime;
    private AttackData _debugAttackData;

    [Header("Attack_Data")]
    [SerializeField]
    private Transform attackSpawnPoint;

    private Coroutine _persistentHitboxCoroutine;

    [SerializeField]
    private NetworkThirdPersonController controller;

    [SerializeField]
    private AttackData[] comboAttackData;
    [Networked]
    public int CurrentAttackIndex { get; set; }

    private PlayerCharacter _playerCharacter;

    private Animator _animator;

    private int _comboIndex;

    private bool _waitingNextCombo;

    private float _comboTimer;

    [Networked]
    public float AttackMoveRemain { get; set; }

    private IStealthHandler _stealth;

    public event Action<int, AttackData> OnAttackSpawned;
    public Transform AttackSpawnPoint => attackSpawnPoint;
    private IUltimateModifier _ultimateModifier;
    public bool IsAttacking
    {
        get;
        private set;
    }

    private static readonly int AttackTriggerHash =
        Animator.StringToHash("AttackTrigger");

    private static readonly int ComboIndexHash =
        Animator.StringToHash("ComboIndex");

    private static readonly int IsAttackingHash =
        Animator.StringToHash("IsAttacking");

    private CharacterActionLock _actionLock;

    //6.16 KYW


    private void Awake()
    {
        _animator = GetComponent<Animator>();

        _playerCharacter = GetComponent<PlayerCharacter>();

        _stealth = GetComponent<IStealthHandler>();
        _ultimateModifier = GetComponent<IUltimateModifier>();
        _actionLock = GetComponent<CharacterActionLock>();

        if (controller == null)
        {
            controller = GetComponent<NetworkThirdPersonController>();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (_waitingNextCombo)
        {
            _comboTimer -= Runner.DeltaTime;

            if (_comboTimer <= 0f)
            {
                ResetCombo();
            }

            return;
        }

        if (IsAttacking)
        {
            _attackStateTimer -= Runner.DeltaTime;

            if (_attackStateTimer <= 0f)
            {
                ResetCombo();
            }
        }
    }

    public void AttackInput()
    {
        if (_actionLock != null && !_actionLock.CanAttack)
            return;

        if (_playerCharacter.IsDashing)
            return;

        AssassinDash dash = GetComponent<AssassinDash>();

        if (dash != null && !dash.DashAttackTimer.ExpiredOrNotRunning(Runner))
        {
            dash.DashAttackTimer = TickTimer.None;

            ForceComboAttack(4);

            return;
        }
        if (_playerCharacter.IsDashing)
            return;


        if (controller == null || !controller.Grounded)
            return;

        if (_playerCharacter != null && _playerCharacter.IsDead)
        {
            return;
        }

        if (!HasStateAuthority)
        {
            return;
        }

        _stealth?.ExitStealth();

        if (_waitingNextCombo)
        {
            _comboIndex++;

            if (_comboIndex > comboAttackData.Length)
                _comboIndex = 1;


            _waitingNextCombo = false;
            _comboTimer = 0f;

            PlayAttack();

            return;
        }

        if (_comboIndex == 0)
        {
            _comboIndex = 1;

            PlayAttack();
        }
    }

    private void PlayAttack()
    {
        CurrentAttackIndex = _comboIndex;
        _attackStateTimer = attackStateTimeout;

        Animator animator =
            GetComponent<Animator>();

        if (animator != null)
        {
            animator.ResetTrigger("Jump");
            animator.SetBool("FreeFall", false);
            animator.SetBool("Grounded", true);
        }

        _actionLock?.Lock(ActionLockType.Move);

        RPC_SetAttackState(true);
        RPC_PlayAttack(_comboIndex);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayAttack(int comboIndex)
    {
        Debug.Log($"RPC_PlayAttack : {comboIndex}");

        _animator.SetInteger(
            ComboIndexHash,
            comboIndex);

        _animator.SetTrigger(
            AttackTriggerHash);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetAttackState(bool value)
    {
        IsAttacking = value;

        _animator.SetBool(
            IsAttackingHash,
            value);
    }

    /// <summary>
    /// 공격 애니메이션 마지막 프레임 이벤트
    /// </summary>
    public void ComboWindowStart()
    {
        Debug.Log("ComboWindowStart");

        if (!HasStateAuthority)
            return;

        RPC_SetAttackState(false);

        _actionLock?.Unlock(ActionLockType.Move);

        _waitingNextCombo = true;

        _comboTimer = comboInputTime;
        _attackStateTimer = 0f;
    }

    private void ResetCombo()
    {
        RPC_SetAttackState(false);

        _actionLock?.Unlock(ActionLockType.Move);

        _comboIndex = 0;
        _waitingNextCombo = false;
        _comboTimer = 0f;
        _attackStateTimer = 0f;

        _animator.SetInteger(ComboIndexHash, 0);
    }

    public void SpawnAttack()
    {
        if (!HasStateAuthority)
            return;

        int attackIndex = CurrentAttackIndex;

        if (attackIndex <= 0)
            return;

        if (attackIndex > comboAttackData.Length)
            return;

        AttackData data =
            comboAttackData[attackIndex - 1];

        switch (data.SpawnType)
        {
            case AttackSpawnType.HitBox:
                SpawnAttackEffect();
                if (data.UsePersistentHitbox)
                {
                    StartPersistentHitBox(data);
                }
                else
                {
                    SpawnHitBox(data);
                }
                break;

            case AttackSpawnType.Projectile:
                SpawnProjectile(data);
                break;
        }
        OnAttackSpawned?.Invoke(CurrentAttackIndex,data);
    }

    private void StartPersistentHitBox(AttackData data)
    {
        _debugAttackData = data;
        _debugAttackEndTime = Time.time + 2f;

        if (_persistentHitboxCoroutine != null)
        {
            StopCoroutine(_persistentHitboxCoroutine);
        }

        _persistentHitboxCoroutine = StartCoroutine(PersistentHitBoxRoutine(data));
    }

    private IEnumerator PersistentHitBoxRoutine(AttackData data)
    {
        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        float elapsed = 0f;

        while (elapsed < data.HitDuration)
        {
            PerformHitBox(data, damagedTargets);

            yield return new WaitForSeconds(data.HitInterval);

            elapsed += data.HitInterval;
        }

        _persistentHitboxCoroutine = null;
    }
    private void PerformHitBox(AttackData data, HashSet<IDamageable> damagedTargets)
    {
        Vector3 center = attackSpawnPoint.position + attackSpawnPoint.forward * (data.Range * 0.5f);

        Collider[] hits = Physics.OverlapBox(center, new Vector3(data.Radius, 1f, data.Range * 0.5f), attackSpawnPoint.rotation);

        foreach (Collider hit in hits)
        {
            if (hit.transform.root == transform.root)
                continue;

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            PlayerCharacter targetPlayer =damageable.GetDamageableObject().GetComponent<PlayerCharacter>();

            if (targetPlayer != null)
            {
                if (targetPlayer.Team ==_playerCharacter.Team)
                {
                    continue;
                }
            }

            if (damagedTargets.Contains(damageable))
                continue;

            damagedTargets.Add(damageable);

            if (data.HitEffect != null)
            {
                Instantiate(data.HitEffect, hit.ClosestPoint(transform.position), Quaternion.identity);
            }

            float multiplier = data.DamagePercent / 100f;

            multiplier *=
                _ultimateModifier
                ?.GetAttackMultiplier(
                    CurrentAttackIndex)
                ?? 1f;

            int finalDamage =
                Mathf.RoundToInt(
                    _playerCharacter.AttackPower *
                    multiplier);

            damageable.TakeDamage(finalDamage, this);

            GameObject targetObject =
                damageable.GetDamageableObject();

            TryApplyRedBuffSlow(targetObject);

            if (data.Knockback > 0f)
            {
                NetworkThirdPersonController controller =
                    targetObject.GetComponent<NetworkThirdPersonController>();

                if (controller != null)
                {
                    Vector3 dir =
                        (targetObject.transform.position -
                         transform.position).normalized;

                    dir.y = 0f;

                    controller.AddKnockback(
                        dir * data.Knockback);
                }
            }

            NetworkObject targetObj =
                targetObject.GetComponent<NetworkObject>();

            if (targetObj != null)
            {
                RPC_HitFeedback(
                    targetObj.Id,
                    data.HitStop,
                    data.CameraShake);
            }
        }
    }

    [Rpc(RpcSources.StateAuthority,RpcTargets.InputAuthority)]
    public void RPC_AttackTargetChanged(NetworkId targetId,float curHP,float maxHP)
    {
        NetworkObject obj = Runner.FindObject(targetId);

        if (obj == null)
            return;

        HUDManager.Instance?.EnemyHUD?.ShowTarget(obj.gameObject,curHP,maxHP);
    }

    private void SpawnHitBox(AttackData data)
    {
        _debugAttackData = data;
        _debugAttackEndTime = Time.time + 2f;

        PerformHitBox(data, new HashSet<IDamageable>());
    }
    private void OnDrawGizmos()
    {
        if (attackSpawnPoint == null)
            return;

        if (_debugAttackData == null)
            return;

        if (Time.time > _debugAttackEndTime)
            return;

        AttackData data = _debugAttackData;

        Vector3 center = attackSpawnPoint.position + attackSpawnPoint.forward * (data.Range * 0.5f);

        Vector3 size = new Vector3(data.Radius * 2f, 2f, data.Range);

        Gizmos.color = Color.red;

        Gizmos.matrix = Matrix4x4.TRS(center, attackSpawnPoint.rotation, Vector3.one);

        Gizmos.DrawWireCube(Vector3.zero, size);
    }
    private void SpawnProjectile(AttackData data)
    {
        if (data.ProjectilePrefab == null)
            return;

        NetworkObject projectilePrefab =
            data.ProjectilePrefab.GetComponent<NetworkObject>();

        if (projectilePrefab == null)
            return;

        Vector3 direction = attackSpawnPoint.forward;
        Quaternion rotation = Quaternion.LookRotation(direction);

        Runner.Spawn(
            projectilePrefab,
            attackSpawnPoint.position,
            rotation,
            Object.InputAuthority,
            (runner, obj) =>
            {
                NetworkProjectileMove projectile =
                    obj.GetComponent<NetworkProjectileMove>();

                if (projectile != null)
                {
                    projectile.Init(this, direction, _playerCharacter.AttackPower * data.DamagePercent / 100);
                }
            });
    }

    public void MoveForwardAttack(float distance)
    {
        if (!HasStateAuthority)
            return;

        AttackMoveRemain += distance;
    }

    public void SpawnAttackEffect()
    {
        if (!HasStateAuthority)
            return;

        RPC_PlayAttackEffect(
            CurrentAttackIndex);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayAttackEffect(int attackIndex)
    {
        if (attackIndex <= 0)
            return;

        if (attackIndex > comboAttackData.Length)
            return;

        AttackData data =
            comboAttackData[attackIndex - 1];
        //수정
        Vector3 spawnPosition =
            attackSpawnPoint.TransformPoint(data.EffectPositionOffset);

        Quaternion spawnRotation =
            transform.rotation *
            Quaternion.Euler(
                data.EffectRotationOffset);

        if (data.SpawnOnGround)
        {
            Vector3 offset =
                attackSpawnPoint.TransformDirection(data.EffectPositionOffset);

            offset.y = 0f;

            Ray ray = new Ray(
                attackSpawnPoint.position + offset + Vector3.up * 2f,
                Vector3.down);

            if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                100f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
            {
                spawnPosition = hit.point;
            }
        }
        //
        GameObject effectPrefab = null;

        // 궁극기 이펙트 우선
        if (_ultimateModifier != null &&
            _ultimateModifier.IsUltimateActive)
        {
            effectPrefab =
                _ultimateModifier.GetOverrideEffect(
                    attackIndex);
        }

        // 기본 이펙트
        if (effectPrefab == null)
        {
            effectPrefab =
                data.AttackEffect;
        }

        if (effectPrefab == null)
            return;

        GameObject effect =
            Instantiate(
                effectPrefab,
                spawnPosition,
                spawnRotation);

        // 장판은 부모 안 붙임
        if (!data.SpawnOnGround)
        {
            effect.transform.SetParent(
                transform,
                true);
        }
    }
    public void DefaultAttack()
    {
    }
    public GameObject GetAttacker()
    {
        return gameObject;
    }

    public void ResetCombatState()
    {
        _actionLock?.ClearAll();

        _comboIndex = 0;
        _waitingNextCombo = false;
        _comboTimer = 0f;
        _attackStateTimer = 0f;

        AttackMoveRemain = 0f;

        IsAttacking = false;
        CurrentAttackIndex = 0;

        _animator.SetBool(IsAttackingHash, false);
        _animator.SetInteger(ComboIndexHash, 0);
    }
    public void ForceComboAttack(int comboIndex)
    {
        if (!HasStateAuthority)
            return;

        _comboIndex = comboIndex;

        PlayAttack();
    }
    public void SpawnShadowAttack(AttackData data,float damageMultiplier)
    {
        Vector3 center = attackSpawnPoint.position +attackSpawnPoint.forward *(data.Range * 0.5f);

        Collider[] hits =
            Physics.OverlapBox(
                center,
                new Vector3(
                    data.Radius,
                    1f,
                    data.Range * 0.5f),
                attackSpawnPoint.rotation);

        HashSet<IDamageable> damagedTargets =
            new HashSet<IDamageable>();

        foreach (Collider hit in hits)
        {
            if (hit.transform.root ==
                transform.root)
                continue;

            IDamageable damageable =
                hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            PlayerCharacter targetPlayer = damageable.GetDamageableObject().GetComponent<PlayerCharacter>();

            if (targetPlayer != null)
            {
                if (targetPlayer.Team == _playerCharacter.Team)
                {
                    continue;
                }
            }

            if (damagedTargets.Contains(
                damageable))
                continue;

            damagedTargets.Add(
                damageable);

            float multiplier =
            data.DamagePercent / 100f;

            int originalDamage =
                Mathf.RoundToInt(
                    _playerCharacter.AttackPower *
                    multiplier);

            int shadowDamage =
                Mathf.RoundToInt(
                    originalDamage *
                    damageMultiplier);

            damageable.TakeDamage(shadowDamage, this);

            GameObject targetObject =
                damageable.GetDamageableObject();

            TryApplyRedBuffSlow(targetObject);

            if (data.Knockback > 0f)
            {
                NetworkThirdPersonController controller =
                    targetObject.GetComponent<NetworkThirdPersonController>();

                if (controller != null)
                {
                    Vector3 dir =
                        (targetObject.transform.position -
                         transform.position).normalized;

                    dir.y = 0f;

                    controller.AddKnockback(
                        dir * data.Knockback);
                }
            }

            NetworkObject targetObj =
                targetObject.GetComponent<NetworkObject>();

            if (targetObj != null)
            {
                RPC_HitFeedback(
                    targetObj.Id,
                    data.HitStop,
                    data.CameraShake);
            }
        }
    }
    public void GetAttackEffectTransform(
    AttackData data,
    out Vector3 position,
    out Quaternion rotation)
    {
        position =
            attackSpawnPoint.position +
            transform.TransformDirection(
                data.EffectPositionOffset);

        rotation =
            transform.rotation *
            Quaternion.Euler(
                data.EffectRotationOffset);
    }

    public void TryApplyRedBuffSlow(GameObject targetObject)
    {
        if (!HasStateAuthority)
            return;

        if (_playerCharacter == null || !_playerCharacter.HasRedBuff)
            return;

        if (_playerCharacter.RedBuffSlowPercent <= 0f ||
            _playerCharacter.RedBuffSlowDuration <= 0f)
            return;

        ISlowable slowable = targetObject.GetComponent<ISlowable>();

        slowable?.ApplySlow(
            _playerCharacter.RedBuffSlowPercent,
            _playerCharacter.RedBuffSlowDuration);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_HitFeedback(NetworkId targetId, float hitStop, float cameraShake)
    {
        NetworkObject targetObj = Runner.FindObject(targetId);

        if (targetObj == null)
            return;

        PlayerCharacter attackerPlayer = GetComponent<PlayerCharacter>();

        if (attackerPlayer != null && attackerPlayer.HasInputAuthority)
        {
            CameraShakeManager.Instance?.Shake(cameraShake);
        }

        PlayerCharacter targetPlayer = targetObj.GetComponent<PlayerCharacter>();

        HitStopController targetHitStop = targetObj.GetComponent<HitStopController>();

        if (targetHitStop != null &&
            targetPlayer != null &&
            targetPlayer.HasInputAuthority)
        {
            targetHitStop.Play(hitStop);
        }
    }
    public void CancelCombo()
    {
        if (!HasStateAuthority)
            return;

        if (_persistentHitboxCoroutine != null)
        {
            StopCoroutine(_persistentHitboxCoroutine);
            _persistentHitboxCoroutine = null;
        }

        _comboIndex = 0;
        _waitingNextCombo = false;
        _comboTimer = 0f;
        _attackStateTimer = 0f;
        CurrentAttackIndex = 0;
        AttackMoveRemain = 0f;

        RPC_SetAttackState(false);

        _animator.SetInteger(ComboIndexHash, 0);

        _actionLock?.Unlock(ActionLockType.Move);
    }
    public void SpawnSkillHitBox(AttackData data)
    {
        if (!HasStateAuthority)
            return;

        PerformHitBox(
            data,
            new HashSet<IDamageable>());
    }
}