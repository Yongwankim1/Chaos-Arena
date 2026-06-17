using Fusion;
using UnityEngine;

public class AssassinSkill : NetworkBehaviour , ISkillQ, ISkillCooldown
{
    [SerializeField]
    private NetworkObject normalShurikenPrefab;

    private AssassinUltimate _ultimate;

    [SerializeField]
    private Transform shurikenSpawnPoint;

    private Animator _animator;

    private static readonly int SkillQHash = Animator.StringToHash("SkillQ");

    private PlayerCharacter _player;

    private CharacterCombat _combat;

    [SerializeField]
    private float manaCost = 20f;

    [SerializeField]
    private float cooldown = 5f;

    public TickTimer CooldownTimer => Cooldown;

    public float CooldownDuration => cooldown;

    [Networked]
    public TickTimer Cooldown { get; set; }

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
        _ultimate = GetComponent<AssassinUltimate>();
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

    public void SpawnShuriken()
    {
        if (!HasStateAuthority)
            return;

        NetworkObject prefab =
            normalShurikenPrefab;

        if (_ultimate != null &&
            _ultimate.IsUltimate &&
            _ultimate.ShadowShurikenPrefab != null)
        {
            prefab =
                _ultimate.ShadowShurikenPrefab;
        }

        Runner.Spawn(
            prefab,
            shurikenSpawnPoint.position,
            Quaternion.LookRotation(transform.forward),
            Object.InputAuthority,
            (runner, obj) =>
            {
                obj.GetComponent<NetworkShuriken>()
                    .Initialize(
                        GetComponent<IAttacker>(),
                        transform.forward);
            });
    }
}