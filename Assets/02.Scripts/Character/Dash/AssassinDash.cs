using Fusion;
using UnityEngine;

public class AssassinDash : NetworkBehaviour, IDash, ISkillCooldown
{
    [SerializeField]
    private float dashDistance = 5f;

    [SerializeField]
    private float dashDuration = 0.1f;

    [SerializeField]
    private float cooldown = 3f;

    public TickTimer CooldownTimer => DashCooldown;

    public float CooldownDuration => cooldown;

    [Networked]
    public TickTimer DashCooldown { get; set; }

    [SerializeField]
    private LayerMask obstacleMask;

    [SerializeField]
    private GameObject startEffect;

    [SerializeField]
    private GameObject endEffect;

    [SerializeField]
    private ParticleSystem dashStartEffect;

    [SerializeField]
    private ParticleSystem dashSmokeEffect;

    [SerializeField]
    private ParticleSystem dashEndEffect;

    private SkinnedMeshRenderer[] _meshes;

    [SerializeField]
    private float dashAttackWindow = 0.5f;

    [Networked]
    public TickTimer DashAttackTimer { get; set; }

    [Networked]
    public float DashRemainDistance { get; set; }

    [Networked]
    public Vector3 DashDirection { get; set; }

    public bool IsDashing => _player != null && _player.IsDashing;

    private PlayerCharacter _player;
    private NetworkCharacterController _controller;
    private CharacterController _cc;
    private AssassinStealth _stealth;

    private void Awake()
    {
        _player = GetComponent<PlayerCharacter>();
        _controller = GetComponent<NetworkCharacterController>();
        _cc = GetComponent<CharacterController>();
        _meshes = GetComponentsInChildren<SkinnedMeshRenderer>(true);

        _stealth = GetComponent<AssassinStealth>();
    }

    public void Dash()
    {
        if (!HasStateAuthority)
            return;

        if (_player.IsDead)
            return;

        if (_player.IsDashing)
            return;

        if (DashCooldown.ExpiredOrNotRunning(Runner) == false)
            return;

        Vector3 direction = transform.forward;

        float distance = GetAvailableDistance(direction);

        if (distance <= 0.1f)
            return;

        _stealth?.ExitStealth();

        DashDirection = direction;

        DashRemainDistance = distance;

        _player.IsDashing = true;

        DashCooldown = TickTimer.CreateFromSeconds(Runner, cooldown);

        RPC_PlayDashStart();

        RPC_SetCharacterVisible(false);
    }

    private float GetAvailableDistance(Vector3 direction)
    {
        Vector3 start = transform.position;

        Vector3 p1 = start + _cc.center + Vector3.up * (-_cc.height * 0.5f + _cc.radius);

        Vector3 p2 = start + _cc.center + Vector3.up * (_cc.height * 0.5f - _cc.radius);

        if (Physics.CapsuleCast(p1, p2, _cc.radius, direction, out RaycastHit hit, dashDistance, obstacleMask))
        {
            return Mathf.Max(hit.distance - 0.3f, 0f);
        }

        return dashDistance;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayDashStart()
    {
        dashStartEffect?.Play();

        dashSmokeEffect?.Play();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayDashEnd()
    {
        dashSmokeEffect?.Stop();

        dashEndEffect?.Play();
    }

    public float GetMoveThisTick()
    {
        if (!HasStateAuthority && !HasInputAuthority)
            return 0f;

        float speed = dashDistance / dashDuration;

        float moveThisTick = speed * Runner.DeltaTime;

        moveThisTick = Mathf.Min(moveThisTick, DashRemainDistance);

        if (HasStateAuthority)
        {
            DashRemainDistance -= moveThisTick;

            if (DashRemainDistance <= 0f)
            {
                DashRemainDistance = 0f;

                _player.IsDashing = false;

                DashAttackTimer = TickTimer.CreateFromSeconds(Runner, dashAttackWindow);

                RPC_PlayDashEnd();

                RPC_SetCharacterVisible(true);
            }
        }

        return moveThisTick;
    }
    private void SetCharacterVisible(bool visible)
    {
        foreach (SkinnedMeshRenderer mesh in _meshes)
        {
            mesh.enabled = visible;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetCharacterVisible(bool visible)
    {
        SetCharacterVisible(visible);
    }
}