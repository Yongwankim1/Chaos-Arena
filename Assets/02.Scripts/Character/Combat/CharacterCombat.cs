using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCombat : NetworkBehaviour, IAttacker
{
    [Header("Combo")]
    [SerializeField]
    private float comboInputTime = 0.5f;

    [Header("Attack_Data")]
    [SerializeField]
    private Transform attackSpawnPoint;

    [SerializeField]
    private AttackData[] comboAttackData;
    [Networked]
    private int CurrentAttackIndex { get; set; }

    private Animator _animator;

    private int _comboIndex;

    private bool _waitingNextCombo;

    private float _comboTimer;

    [Networked]
    public float AttackMoveRemain { get; set; }
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

    private void Awake()
    {
        _animator = GetComponent<Animator>();
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
        }
    }

    public void AttackInput()
    {
        Debug.Log(
            $"AttackInput | Combo:{_comboIndex} State:{HasStateAuthority} Input:{HasInputAuthority}");

        if (!HasStateAuthority)
        {
            Debug.Log("AttackInput Return");
            return;
        }

        if (_waitingNextCombo)
        {
            _comboIndex++;

            if (_comboIndex > comboAttackData.Length)
                _comboIndex = 1;

            Debug.Log(
                $"Next Combo : {_comboIndex}");

            _waitingNextCombo = false;
            _comboTimer = 0f;

            PlayAttack();

            return;
        }

        if (_comboIndex == 0)
        {
            _comboIndex = 1;

            Debug.Log(
                $"Start Combo : {_comboIndex}");

            PlayAttack();
        }
    }

    private void PlayAttack()
    {
        CurrentAttackIndex = _comboIndex;

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

        _waitingNextCombo = true;

        _comboTimer = comboInputTime;
    }

    private void ResetCombo()
    {
        RPC_SetAttackState(false);

        _comboIndex = 0;

        _waitingNextCombo = false;

        _comboTimer = 0f;

        _animator.SetInteger(
            ComboIndexHash,
            0);
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
                SpawnHitBox(data);
                break;

            case AttackSpawnType.Projectile:
                SpawnProjectile(data);
                break;
        }
    }
    private void SpawnHitBox(AttackData data)
    {
        Vector3 center =
            attackSpawnPoint.position +
            attackSpawnPoint.forward *
            (data.Range * 0.5f);

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
            if (hit.transform.root == transform.root)
                continue;

            IDamageable damageable =
                hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            if (damagedTargets.Contains(damageable))
                continue;

            damagedTargets.Add(damageable);

            damageable.TakeDamage(
                Mathf.RoundToInt(data.Damage),
                this);
        }
    }
    private void OnDrawGizmos()
    {
        if (attackSpawnPoint == null)
            return;

        if (comboAttackData == null ||
            comboAttackData.Length == 0)
            return;

        AttackData data = comboAttackData[0];

        Vector3 center =
            attackSpawnPoint.position +
            attackSpawnPoint.forward *
            (data.Range * 0.5f);

        Vector3 size =
            new Vector3(
                data.Radius * 2f,
                2f,
                data.Range);

        Gizmos.color = Color.red;

        Gizmos.matrix =
            Matrix4x4.TRS(
                center,
                attackSpawnPoint.rotation,
                Vector3.one);

        Gizmos.DrawWireCube(
            Vector3.zero,
            size);
    }
    private void SpawnProjectile(
    AttackData data)
    {
        Runner.Spawn(
            data.ProjectilePrefab.GetComponent<NetworkObject>(),
            attackSpawnPoint.position,
            transform.rotation,
            Object.InputAuthority);
    }

    public void MoveForwardAttack(float distance)
    {
        if (!HasStateAuthority)
            return;

        AttackMoveRemain += distance;
    }
    public void DefaultAttack()
    {
    }
    public GameObject GetAttacker()
    {
        return gameObject;
    }
}