using Fusion;
using UnityEngine;

public class CharacterCombat : NetworkBehaviour
{
    [Header("Combo")]
    [SerializeField]
    private float comboInputTime = 0.5f;

    private Animator _animator;

    private int _comboIndex;

    private bool _waitingNextCombo;

    private float _comboTimer;

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
        if (!HasStateAuthority)
            return;

        if (_waitingNextCombo)
        {
            _comboIndex++;

            if (_comboIndex > 5)
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
}