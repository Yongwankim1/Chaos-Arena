using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ClassData",
    menuName = "Game Data/Class Data")]
public class ClassData : ScriptableObject
{
    [Header("기본 정보")]
    public CharacterClassType classType;
    public string className;

    [Header("기본 능력치")]
    public float maxHP;
    public float maxMana;

    public float walkSpeed;
    public float sprintSpeed;

    public float attackPower;

}