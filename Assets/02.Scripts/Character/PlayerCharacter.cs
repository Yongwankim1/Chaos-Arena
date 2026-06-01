using Fusion;
using UnityEngine;

public class PlayerCharacter : NetworkBehaviour
{
    [SerializeField]
    private ClassData classData;

    [Networked]
    public float CurrentHP { get; set; }

    [Networked]
    public float CurrentMana { get; set; }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            CurrentHP = classData.maxHP;
            CurrentMana = classData.maxMana;
        }

        var controller =
            GetComponent<NetworkThirdPersonController>();

        if (controller != null)
        {
            controller.MoveSpeed =
                classData.moveSpeed;
        }
    }
}