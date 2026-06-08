using Fusion;
using UnityEngine;

public class PlayerCharacter : NetworkBehaviour
{
    private ClassData _classData;

    [Networked]
    public float CurrentHP { get; set; }

    [Networked]
    public float CurrentMana { get; set; }
    public float MaxHP => _classData.maxHP;
    public float MaxMana => _classData.maxMana;
    public void Initialize(
        ClassData classData)
    {
        _classData = classData;

        if (HasStateAuthority)
        {
            CurrentHP =
                classData.maxHP;

            CurrentMana =
                classData.maxMana;
        }

        ApplyMovementStat();

        if (HasInputAuthority)
        {
            HUDManager.Instance?.BindPlayer(this);
        }
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