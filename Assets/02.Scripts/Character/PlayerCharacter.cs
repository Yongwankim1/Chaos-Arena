using Fusion;
using UnityEngine;

public class PlayerCharacter : NetworkBehaviour
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
}