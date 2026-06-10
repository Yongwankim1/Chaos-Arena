using Fusion;
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

    [Networked]
    public TeamType Team { get; set; }

    [Networked]
    public NetworkBool IsDead { get; set; }

    private IAttacker _lastAttacker;

    public float MaxHP =>
        _classData.maxHP;

    public float MaxMana =>
        _classData.maxMana;

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
    public void TakeDamage(
     int damage,
     IAttacker attacker)
    {
        if (!HasStateAuthority)
            return;

        _lastAttacker =
            attacker;

        CurrentHP =
            Mathf.Max(
                0,
                CurrentHP - damage);

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
    public void Respawn(
       Vector3 position)
    {
        if (!HasStateAuthority)
            return;

        CurrentHP =
            MaxHP;

        CurrentMana =
            MaxMana;

        IsDead =
            false;

        NetworkCharacterController controller =
            GetComponent<NetworkCharacterController>();

        if (controller != null)
        {
            controller.Teleport(position);
        }
        else
        {
            transform.position =
                position;
        }

        RPC_ResetCharacter();
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ResetCharacter()
    {
        if (_animator == null)
            return;

        _animator.Rebind();
        _animator.Update(0f);
    }
}