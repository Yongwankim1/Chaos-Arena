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

    // =========================
    // 추후 구현 예정
    // =========================

    // [Header("대쉬")]
    // public DashData dashData;

    // [Header("기본 공격")]
    // public List<BasicAttackData> basicAttackList;

    // [Header("스킬")]
    // public List<SkillData> skillList;

    // [Header("스킬 연계")]
    // public List<SkillRelationData> skillRelationList;

    // [Header("무기")]
    // public List<WeaponData> weaponList;

    // [Header("직업 이펙트")]
    // public List<EffectData> classEffectSet;
}