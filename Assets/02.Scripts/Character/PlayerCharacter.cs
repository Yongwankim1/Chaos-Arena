using Fusion;
using UnityEngine;

public class PlayerCharacter : NetworkBehaviour, IDamageable
{
    private ClassData _classData;
    [Networked]
    public CharacterClassType ClassType { get; set; }
    private bool _isInitialized;
    [Networked]
    public float CurrentHP { get; set; }

    [Networked]
    public float CurrentMana { get; set; }

    public float MaxHP =>
        _classData.maxHP;

    public float MaxMana =>
        _classData.maxMana;
    public override void Spawned()
    {
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
        Debug.Log(
            $"TakeDamage Start | Damage:{damage} State:{HasStateAuthority}");

        if (!HasStateAuthority)
        {
            Debug.Log(
                "TakeDamage Return - No StateAuthority");

            return;
        }

        CurrentHP -= damage;

        Debug.Log(
            $"HP Changed : {CurrentHP}");

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;

            Debug.Log("Dead");

            Die();
        }
    }
    private void Die()
    {
        Debug.Log("Player Dead");
    }
}