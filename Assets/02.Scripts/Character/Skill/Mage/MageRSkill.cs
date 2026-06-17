using Fusion;
using UnityEngine;

public class MageRSkill : NetworkBehaviour, ISkillR

{
    [SerializeField]
    private NetworkObject Prefab;

    [SerializeField]
    private Transform spawnPoint;

    private Animator _animator;

    private static readonly int SkillRHash = Animator.StringToHash("SkillR");

    private PlayerCharacter _player;

    private CharacterCombat _combat;

    private NetworkObject spawnedLaser;

    [SerializeField]
    private float manaCost = 20f;

    [SerializeField]
    private float cooldown = 5f;

    [Networked]
    public TickTimer Cooldown { get; set; }

    public float CooldownDuration => cooldown;
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
    }
    public void UseR()
    {
        if (!HasStateAuthority)
            return;

        if (_player.IsDead)
            return;

        if (!Cooldown.ExpiredOrNotRunning(Runner))
            return;

        if (!_player.UseMana(manaCost))
            return;


        Cooldown = TickTimer.CreateFromSeconds(Runner, cooldown);

        RPC_PlaySkillR();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlaySkillR()
    {
        _animator.SetTrigger(SkillRHash);
    }


    public void SpawnLaserEffect()
    {
        if (!HasStateAuthority)
            return;

        spawnedLaser = Runner.Spawn(Prefab, spawnPoint.position,
            Quaternion.LookRotation(transform.forward), Object.InputAuthority,
            (runner, obj) => {
                obj.transform.SetParent(spawnPoint,true);
                obj.GetComponent<MageLaserAttack>().Init(GetComponent<IAttacker>());
            });
    }

    public void EndLaserEffect()
    {
        if (!HasStateAuthority)
            return;

        if (spawnedLaser == null)
            return;

        Runner.Despawn(spawnedLaser);
        spawnedLaser = null;
    }
}
