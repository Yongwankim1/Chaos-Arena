using UnityEngine;

[System.Serializable]
public class AttackData
{
    public AttackSpawnType SpawnType;

    [Header("Damage")]
    public float DamagePercent = 100f;

    public float Range;

    public float Radius;

    [Header("Effect")]
    public GameObject AttackEffect;

    public Vector3 EffectPositionOffset;

    public Vector3 EffectRotationOffset;

    public GameObject HitEffect;

    public GameObject ProjectilePrefab;
}