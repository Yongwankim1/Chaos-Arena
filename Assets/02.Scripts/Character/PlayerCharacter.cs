using Fusion;
using Photon.Realtime;
using UnityEngine;

public class PlayerCharacter : NetworkBehaviour, IDamageable, IDeathHandler
{
    private ClassData _classData;
    [Networked]
    public CharacterClassType ClassType { get; set; }
    private bool _isInitialized;
    [Networked]
    public float CurrentHP { get; set; }

    [Networked]
    public float CurrentMana { get; set; }

    [SerializeField]
    private float manaRegenDelay = 3f;

    [Networked]
    private TickTimer ManaRegenDelayTimer { get; set; }

    [SerializeField]
    private float manaRegenPerSecond = 8f;

    [Networked]
    public TeamType Team { get; set; }

    [Networked]
    public NetworkBool IsDead { get; set; }
    [Networked]
    public NetworkBool IsDashing { get; set; }

    private IAttacker _lastAttacker;

    public float MaxHP => _classData.maxHP;

    public float MaxMana => _classData.maxMana;

    public float AttackPower => _classData.attackPower;

    private static readonly int DieHash =
    Animator.StringToHash("Die");

    private Animator _animator;
    public override void Spawned()
    {
        _animator = GetComponent<Animator>();
        Debug.Log(
            $"[Player] ObjectState:{Object.HasStateAuthority} ObjectInput:{Object.HasInputAuthority}");

        Debug.Log(
            $"[Player] State:{HasStateAuthority} Input:{HasInputAuthority}");

        Debug.Log(
            $"Spawned : {ClassType}");
    }
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (IsDead)
            return;

        if (ManaRegenDelayTimer.IsRunning && !ManaRegenDelayTimer.Expired(Runner))
            return;

        RecoverMana(manaRegenPerSecond * Runner.DeltaTime);
    }
    public bool UseMana(float amount)
    {
        if (!HasStateAuthority)
            return false;

        if (CurrentMana < amount)
            return false;

        CurrentMana -= amount;

        DelayManaRegen();

        return true;
    }

    public void DelayManaRegen()
    {
        if (!HasStateAuthority)
            return;

        ManaRegenDelayTimer = TickTimer.CreateFromSeconds(Runner, manaRegenDelay);
    }

    public void RecoverMana(float amount)
    {
        if (!HasStateAuthority)
            return;

        CurrentMana = Mathf.Min(CurrentMana + amount, MaxMana);
    }

    private void LocalInitialize()
    {
        _classData =
            ClassDataManager.GetData(
                ClassType);

        ApplyMovementStat();

        if (HasStateAuthority)
        {
            CurrentHP =
                _classData.maxHP;

            CurrentMana =
                _classData.maxMana;
        }

        if (HasInputAuthority)
        {
            Debug.Log(
                $"HUD Bind : {ClassType}");

            HUDManager.Instance
                ?.BindPlayer(this);
        }
    }
    public override void Render()
    {
        if (_isInitialized)
            return;

        if (ClassType == CharacterClassType.None)
            return;

        if (!ClassDataManager.IsLoaded(
                ClassType))
            return;

        _isInitialized = true;

        LocalInitialize();
    }
    private void ApplyMovementStat()
    {
        var controller =
            GetComponent<NetworkThirdPersonController>();

        if (controller != null)
        {
            controller.MoveSpeed =
                _classData.walkSpeed;

            controller.SprintSpeed =
                _classData.sprintSpeed;
        }

        var cc =
            GetComponent<NetworkCharacterController>();

        if (cc != null)
        {
            cc.maxSpeed =
                _classData.sprintSpeed;
        }
    }
    public void TakeDamage(int damage, IAttacker attacker)
    {
        if (!HasStateAuthority)
            return;

        DelayManaRegen();

        _lastAttacker = attacker;

        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        if (CurrentHP <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        if (IsDead)
            return;

        IsDead = true;
        GetComponent<Buff>().Init();
        RPC_PlayDie();

        HandleDeath(_lastAttacker);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayDie()
    {
        if (_animator == null)
            return;

        _animator.SetTrigger(
            DieHash);
    }

    public void HandleDeath(IAttacker attacker)
    {
        RoundManager.Instance.OnPlayerDeath(this, attacker);
    }
    public void Respawn(Vector3 position)
    {
        if (!HasStateAuthority)
            return;
        //GetComponent<Buff>().Init();
        Vector3 oldPosition = transform.position;

        CurrentHP = MaxHP;
        CurrentMana = MaxMana;
        IsDead = false;

        CharacterCombat combat = GetComponent<CharacterCombat>();

        if (combat != null)
        {
            combat.ResetCombatState();
        }

        NetworkCharacterController controller = GetComponent<NetworkCharacterController>();

        if (controller != null)
        {
            controller.Teleport(position);
        }
        else
        {
            transform.position = position;
        }

        RPC_ResetCharacter(oldPosition, position);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ResetCharacter(Vector3 oldPosition, Vector3 newPosition)
    {
        if (_animator != null)
        {
            _animator.Rebind();
            _animator.Update(0f);
        }

        NetworkThirdPersonController controller3rd = GetComponent<NetworkThirdPersonController>();

        if (controller3rd == null)
            return;

        if (!controller3rd.HasInputAuthority)
            return;

        var cam = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();

        if (cam == null)
            return;
        Debug.Log( $"Camera Follow : {cam.Target.TrackingTarget.name}");


        Vector3 delta = newPosition - oldPosition;

        cam.OnTargetObjectWarped(transform, delta);
    }
}