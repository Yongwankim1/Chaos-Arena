using Fusion;
using UnityEngine;

public class MageQSkill : NetworkBehaviour
{
    [SerializeField]
    private NetworkObject lightingPrefab;

    [SerializeField]
    private Transform lightingEffectSpawnPoint;

    private Animator _animator;

    private static readonly int SkillQHash = Animator.StringToHash("SkillQ");

    private PlayerCharacter _player;

    private CharacterCombat _combat;

    [SerializeField]
    private float manaCost = 20f;

    [SerializeField]
    private float cooldown = 5f;

    [Networked]
    public TickTimer Cooldown { get; set; }

    public float CooldownDuration => cooldown;
    private AssassinStealth _stealth;




    public float CooldownNormalized
    {
        get
        {
            if (Cooldown.ExpiredOrNotRunning(Runner))
                return 0f;

            float remain = Cooldown.RemainingTime(Runner) ?? 0f;

            return remain / cooldown;
        }
    }

    private void Awake()
    {
        _player = GetComponent<PlayerCharacter>();
        _combat = GetComponent<CharacterCombat>();
        _animator = GetComponent<Animator>();
        _stealth = GetComponent<AssassinStealth>();
    }

    public void UseQ()
    {
        if (!HasStateAuthority)
            return;

        if (_player.IsDead)
            return;

        if (!Cooldown.ExpiredOrNotRunning(Runner))
            return;

        if (!_player.UseMana(manaCost))
            return;

        _stealth?.ExitStealth();

        Cooldown = TickTimer.CreateFromSeconds(Runner, cooldown);

        RPC_PlaySkillQ();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlaySkillQ()
    {
        _animator.SetTrigger(SkillQHash);
    }

    public void SpawnExplosion()
    {
        if (!HasStateAuthority)
            return;

        Runner.Spawn(lightingPrefab, lightingEffectSpawnPoint.position, Quaternion.LookRotation(transform.forward), Object.InputAuthority, (runner, obj) => { obj.GetComponent<MageLightningProjectile>().Init(GetComponent<IAttacker>()); });
    }
}
