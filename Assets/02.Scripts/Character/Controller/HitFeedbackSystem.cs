
using UnityEngine;

public static class HitFeedbackSystem
{
    public static void Apply(IAttacker attacker, IDamageable target, AttackData data)
    {
        if (attacker == null || target == null || data == null)
            return;

        GameObject attackerObj = attacker.GetAttacker();
        GameObject targetObj = target.GetDamageableObject();

        if (attackerObj == null || targetObj == null)
            return;

        ApplyHitStop(attackerObj, targetObj, data.HitStop);
        ApplyCameraShake(attackerObj, data.CameraShake);
        ApplyKnockback(attackerObj, targetObj, data.Knockback);
    }

    private static void ApplyHitStop(GameObject attacker, GameObject target, float duration)
    {
        attacker.GetComponent<HitStopController>()?.Play(duration);
        target.GetComponent<HitStopController>()?.Play(duration);
    }

    private static void ApplyCameraShake(GameObject attacker, float intensity)
    {
        PlayerCharacter player = attacker.GetComponent<PlayerCharacter>();

        if (player == null)
            return;

        if (!player.HasInputAuthority)
            return;

        Debug.Log($"Camera Shake : {intensity}");

        CameraShakeManager.Instance?.Shake(intensity);
    }

    private static void ApplyKnockback(GameObject attacker, GameObject target, float power)
    {
        if (power <= 0f)
            return;

        NetworkThirdPersonController controller = target.GetComponent<NetworkThirdPersonController>();

        if (controller == null)
            return;

        Vector3 dir = (target.transform.position - attacker.transform.position).normalized;

        dir.y = 0f;

        controller.AddKnockback(dir * power);
    }
}