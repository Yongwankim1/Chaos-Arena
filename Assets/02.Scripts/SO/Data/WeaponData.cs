using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Weapon_New", menuName = "RPG Data/Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("기본 정보")]
    public string weaponId;
    public string weaponName;
    public WeaponType weaponType;
    public List<JobType> jobs = new List<JobType>();
    public Sprite icon;

    [Header("전투 수치")]
    public int damage;
    public float attackRange;
    public float attackRate;
    public float criticalChance;
    public float criticalMulitiplier = 1.5f;

    [Header("무기 특성")]
    public int magazineSize;
    public float reloadTime;
    public bool useAmmo;
    public bool isMelee;

    [Header("프리팹 및 이펙트")]
    public GameObject weaponPrefab;
    public GameObject projectilePrefab;
    public ParticleSystem fireEffect;
    public AudioClip attackSound;
}
