using UnityEngine;

[System.Serializable]
public class AttackData
{
    public AttackSpawnType SpawnType;

    public float Damage;

    public float Range;

    public float Radius;

    [Header("Effect")]
    public GameObject AttackEffect;

    public Vector3 EffectPositionOffset;

    public Vector3 EffectRotationOffset;

    public GameObject HitEffect;

    public GameObject ProjectilePrefab;
}