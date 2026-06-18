using UnityEngine;

[System.Serializable]
public class AttackData
{
    public AttackSpawnType SpawnType;

    [Header("Damage")]
    public float DamagePercent = 100f;

    public float Range;

    public float Radius;

    [Header("Persistent Hitbox")]
    public bool UsePersistentHitbox;

    public float HitDuration = 0.3f;

    public float HitInterval = 0.05f;

    [Header("Effect")]
    public GameObject AttackEffect;

    public Vector3 EffectPositionOffset;

    public Vector3 EffectRotationOffset;

    public GameObject HitEffect;

    public GameObject ProjectilePrefab;

    public bool SpawnOnGround;

    [Header("Hit Feedback")]
    public float HitStop = 0.03f;
    public float CameraShake = 0.3f;
    public float Knockback = 0.2f;
}