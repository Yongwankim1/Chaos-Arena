using Fusion;
using UnityEngine;

public class MageQSkill : NetworkBehaviour, ISkillQ , ISkillCooldown
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

    [SerializeField] private float damagePercent = 3f;

    [Networked]
    public TickTimer Cooldown { get; set; }
    public TickTimer CooldownTimer => Cooldown;
    public float CooldownDuration => cooldown;


    [Header("ø¨√‚¿Ã∆Â∆Æ")]
    [SerializeField] Transform magicSoket;
    [SerializeField] ParticleSystem orbEffect;
    [SerializeField] ParticleSystem magicCircle;


    [SerializeField] Transform buffPos;
    [SerializeField] private int orbMoveFrame = 12;

    private ParticleSystem orb;
    private bool isOrbMoving;
    private int orbMoveCurrentFrame;
    private Vector3 orbMoveStartPosition;
    private Vector3 orbMoveEndPosition;
    private CharacterActionLock _actionLock;

    private NetworkThirdPersonController _controller;
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
        _actionLock = GetComponent<CharacterActionLock>();
        _controller = GetComponent<NetworkThirdPersonController>();
    }

    private void Update()
    {
        UpdateOrbMove();
    }

    public void UseQ()
    {
        if (!HasStateAuthority)
            return;

        if (_player.IsDead)
            return;
        if (!_controller.Grounded) return;

        if (!Cooldown.ExpiredOrNotRunning(Runner))
            return;

        if (!_player.UseMana(manaCost))
            return;

        Cooldown = TickTimer.CreateFromSeconds(Runner, cooldown);

        RPC_PlaySkillQ();
        _actionLock?.Lock(ActionLockType.Move);
        _actionLock?.Lock(ActionLockType.Attack);
        _actionLock?.Lock(ActionLockType.Dash);
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
        float finalDamage = _player.AttackPower * damagePercent;
        Runner.Spawn(lightingPrefab, lightingEffectSpawnPoint.position, Quaternion.LookRotation(transform.forward), Object.InputAuthority, (runner, obj) => 
        { obj.GetComponent<MageLightningProjectile>().Init(GetComponent<IAttacker>(), (int)finalDamage); });
    }


    public void StartMagicCircle()
    {
        RPC_OnMagicCircle();
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_OnMagicCircle()
    {
        if (magicCircle == null)
            return;
        ParticleSystem ps = Instantiate(magicCircle, buffPos.position, Quaternion.identity, buffPos);

        ps.Play();

        Destroy(ps.gameObject, GetParticleLifeTime(ps));
    }
    public void GatheringEnergy()
    {
        RPC_GateringEnergy();
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_GateringEnergy()
    {
        if (orbEffect == null || magicSoket == null)
            return;

        if (orb != null)
        {
            Destroy(orb.gameObject);
        }

        isOrbMoving = false;
        orb = Instantiate(orbEffect, magicSoket.position, Quaternion.identity,magicSoket);
        orb.Play();
    }

    public void EnergyMove()
    {
        RPC_EnergyMove();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_EnergyMove()
    {
        if (orb == null || lightingEffectSpawnPoint == null)
            return;

        isOrbMoving = true;
        orbMoveCurrentFrame = 0;
        orbMoveStartPosition = orb.transform.position;
        orbMoveEndPosition = lightingEffectSpawnPoint.position;
    }

    private void UpdateOrbMove()
    {
        if (!isOrbMoving)
            return;

        if (orb == null)
        {
            isOrbMoving = false;
            return;
        }

        orbMoveCurrentFrame++;

        float t = orbMoveFrame <= 0
            ? 1f
            : orbMoveCurrentFrame / (float)orbMoveFrame;

        t = Mathf.Clamp01(t);
        orb.transform.position = Vector3.Lerp(orbMoveStartPosition, orbMoveEndPosition, t);

        if (t < 1f)
            return;

        isOrbMoving = false;
        Destroy(orb.gameObject);
        orb = null;
    }

    private float GetParticleLifeTime(ParticleSystem root)
    {
        float lifeTime = 0f;
        ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            lifeTime = Mathf.Max(
                lifeTime,
                main.duration + main.startLifetime.constantMax
            );
        }

        return lifeTime;
    }

    public void CanMove()
    {
        _actionLock?.Unlock(ActionLockType.Move);
        _actionLock?.Unlock(ActionLockType.Attack);
        _actionLock?.Unlock(ActionLockType.Dash);
        _actionLock?.Unlock(ActionLockType.Jump);
    }
}
