using Fusion;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(NetworkCharacterController))]
public class NetworkThirdPersonController : NetworkBehaviour
{
    [Header("Player")]
    public float MoveSpeed = 20.0f;
    public float SprintSpeed = 40.0f;
    public float RotationSmoothTime = 0.01f;
    public float SpeedChangeRate = 20.0f;
    [SerializeField]
    private float attackMoveSpeed = 100f;

    [Header("Jump")]
    public float JumpCooldown = 0.25f;
    public float FallTimeout = 0.02f;

    private bool _wasGrounded;

    private float _landingLockTimer;

    private float _jumpCooldownTimer;
    private float _fallTimeoutDelta;

    [Header("Attack")]
    private int _animIDAttack;
    private CharacterCombat _combat;
    private int _comboStep;
    private Vector3 _knockbackVelocity;

    [Header("Ground")]
    public bool Grounded = true;
    private float _airTime;
    public bool IgnoreLandAnimation;
    [Header("Camera")]
    public Transform PlayerCameraRoot;
    public float TopClamp = 70f;
    public float BottomClamp = -30f;
    public float CameraAngleOverride = 0f;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    private float _speed;
    private float _animationBlend;
    private float _targetRotation;
    private float _rotationVelocity;

    [Header("Skill")]
    private ISkillQ _skillQ;
    private ISkillE _skillE;
    private ISkillR _skillR;

    private Animator _animator;
    private NetworkCharacterController _controller;
    private PlayerCharacter _playerCharacter;
    private IDash _dash;

    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDJumpLand;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private const float _threshold = 0.01f;
    [SerializeField]
    float lookMultiplier = 1.2f;
    private NetworkInputData _lastInput;

    private CharacterActionLock _actionLock;

    [Networked]
    private float AnimatedSpeed { get; set; }

    [Networked]
    private NetworkBool AnimatedGrounded { get; set; }

    [Networked]
    private NetworkBool AnimatedFreeFall { get; set; }

    [Networked]
    private NetworkBool RotationOnly { get; set; }

    private float _localAnimatedSpeed;
    private bool _localAnimatedGrounded = true;
    private bool _localAnimatedFreeFall;

    public override void Spawned()
    {

        _animator = GetComponent<Animator>();
        _controller = GetComponent<NetworkCharacterController>();
        _combat = GetComponent<CharacterCombat>();
        _playerCharacter = GetComponent<PlayerCharacter>();
        _actionLock = GetComponent<CharacterActionLock>();

        _skillQ = GetComponent<ISkillQ>();
        _skillE = GetComponent<ISkillE>();
        _skillR = GetComponent<ISkillR>();

        foreach (var component in GetComponents<MonoBehaviour>())
        {
            if (component is IDash dash)
            {
                _dash = dash;
                break;
            }
        }

        AssignAnimationIDs();

        _fallTimeoutDelta = FallTimeout;
        _jumpCooldownTimer = 0f;

        _wasGrounded = true;
        _landingLockTimer = 0f;

        if (HasInputAuthority)
        {
            GetComponent<NetworkStarterAssetsInput>()?.RegisterAsLocal();

            var cam = FindFirstObjectByType<CinemachineCamera>(
                FindObjectsInactive.Include);

            if (cam != null)
            {
                cam.Target.TrackingTarget = PlayerCameraRoot;
            }

            _cinemachineTargetYaw = transform.eulerAngles.y;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (_playerCharacter != null && _playerCharacter.IsDead)
            return;

        if (!GetInput(out NetworkInputData input))
            return;

        _lastInput = input;

        GroundedCheck();
        JumpAndGravity(input);

        ApplyKnockback();

        if (_combat != null && _combat.IsAttacking)
        {
            if (_combat.AttackMoveRemain > 0f)
            {
                ApplyAttackDash();
            }
        }
        else if (_playerCharacter.IsDashing)
        {
            ApplyClassDash();
        }
        else
        {
            Move(input);
        }

        Attack(input);
        Dash(input);
        GroundedCheck();
        CaptureAnimationState();
        UpdateAnimatorParameters();
        Skill(input);
    }

    private void LateUpdate()
    {
        if (HasInputAuthority)
        {
            CameraRotation(_lastInput);
        }
    }
    private void ApplyAttackDash()
    {
        if (_combat == null)
            return;

        if (_combat.AttackMoveRemain <= 0f)
            return;

        float moveThisTick =
            attackMoveSpeed *
            Runner.DeltaTime;

        moveThisTick =
            Mathf.Min(
                moveThisTick,
                _combat.AttackMoveRemain);

        _combat.AttackMoveRemain -=
            moveThisTick;

        _controller.ForceMove(
            transform.forward *
            moveThisTick);
    }

    private void GroundedCheck()
    {
        bool groundedNow = _controller.Grounded;

        if (IgnoreLandAnimation)
        {
            _airTime = 0f;
            Grounded = groundedNow;
            _wasGrounded = groundedNow;
            return;
        }

        if (!groundedNow)
        {
            _airTime += Runner.DeltaTime;
        }
        else if (!_wasGrounded)
        {
            bool shouldPlayLand = _airTime >= 0.15f;

            if (shouldPlayLand)
            {
                if (_playerCharacter != null &&
                    _playerCharacter.IsUsingUltimate)
                {
                    return;
                }

                if (HasStateAuthority)
                {
                    if (_combat == null ||
                        !_combat.IsAttacking)
                    {
                        RPC_PlayLandAnimation();
                    }
                }
            }

            _airTime = 0f;
        }

        Grounded = groundedNow;

        _wasGrounded = groundedNow;
    }

    private void CameraRotation(NetworkInputData input)
    {
        if (!HasInputAuthority)
            return;

        if (input.Look.sqrMagnitude >= _threshold)
        {
            _cinemachineTargetYaw += input.Look.x * lookMultiplier;
            _cinemachineTargetPitch += input.Look.y * lookMultiplier;
        }

        _cinemachineTargetPitch =
            Mathf.Clamp(
                _cinemachineTargetPitch,
                BottomClamp, TopClamp);

        PlayerCameraRoot.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0f);

    }

    private void Move(NetworkInputData input)
    {
        if (_playerCharacter != null && _playerCharacter.IsDashing)
            return;

        if (_actionLock != null && !_actionLock.CanMove)
        {
            RotateToYaw(input.Yaw);
            return;
        }

        float targetSpeed =
            input.Sprint
                ? SprintSpeed
                : MoveSpeed;

        if (input.Move == Vector2.zero)
            targetSpeed = 0.0f;

        float currentHorizontalSpeed =
            new Vector3(
                _controller.Velocity.x,
                0.0f,
                _controller.Velocity.z)
            .magnitude;

        float speedOffset = 0.1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(
                currentHorizontalSpeed,
                targetSpeed,
                Runner.DeltaTime * SpeedChangeRate);

            _speed =
                Mathf.Round(_speed * 1000f) /
                1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(
            _animationBlend,
            targetSpeed,
            Runner.DeltaTime * SpeedChangeRate);

        if (_animationBlend < 0.01f)
            _animationBlend = 0f;

        Vector3 moveDirection = Vector3.zero;

        if (input.Move != Vector2.zero)
        {
            Vector3 inputDirection =
                new Vector3(
                    input.Move.x,
                    0.0f,
                    input.Move.y).normalized;

            _targetRotation =
         Mathf.Atan2(
             inputDirection.x,
             inputDirection.z) *
         Mathf.Rad2Deg +
         input.Yaw;

            float rotation =
                Mathf.SmoothDampAngle(
                    transform.eulerAngles.y,
                    _targetRotation,
                    ref _rotationVelocity,
                    RotationSmoothTime);

            transform.rotation =
                Quaternion.Euler(
                    0.0f,
                    rotation,
                    0.0f);

            Vector3 targetDirection =
                Quaternion.Euler(
                    0.0f,
                    _targetRotation,
                    0.0f) *
                Vector3.forward;

            moveDirection =
                targetDirection.normalized *
                _speed;
        }

        _controller.Move(
            moveDirection *
            Runner.DeltaTime);
    }

    public void SetRotationOnly(bool value)
    {
        if (!HasStateAuthority)
            return;

        RotationOnly = value;
    }

    private void RotateToYaw(float yaw)
    {
        if (!RotationOnly)
            return;

        float rotation =
            Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                yaw,
                ref _rotationVelocity,
                RotationSmoothTime);

        transform.rotation =
            Quaternion.Euler(
                0.0f,
                rotation,
                0.0f);
    }
    private void JumpAndGravity(NetworkInputData input)
    {
        if (_playerCharacter != null && _playerCharacter.IsUsingUltimate)
        {
            return;
        }
        if (_actionLock != null && !_actionLock.CanJump)
        {
            return;
        }
        if (_jumpCooldownTimer > 0f)
            _jumpCooldownTimer -= Runner.DeltaTime;

        if (_landingLockTimer > 0f)
            _landingLockTimer -= Runner.DeltaTime;

        if (_combat != null && _combat.IsAttacking)
        {
            return;
        }
        if (_playerCharacter.IsDashing)
            return;
        if (Grounded)
        {
            _fallTimeoutDelta = FallTimeout;

            if (input.Jump && _jumpCooldownTimer <= 0f && _landingLockTimer <= 0f)
            {
                float previousVerticalVelocity = _controller.Velocity.y;
                _controller.Jump();

                bool jumpStarted = _controller.Velocity.y > previousVerticalVelocity;

                if (jumpStarted)
                {
                    _airTime = 0f;

                    _controller.Grounded = false;
                    Grounded = false;

                    if (HasStateAuthority)
                    {
                        RPC_PlayJumpAnimation();
                    }

                    _jumpCooldownTimer = JumpCooldown;
                }
            }
        }
        else
        {
            if (_fallTimeoutDelta > 0f)
            {
                _fallTimeoutDelta -= Runner.DeltaTime;
            }
        }
    }
    private void CaptureAnimationState()
    {
        bool freeFall =!_playerCharacter.IsUsingUltimate &&!Grounded && _fallTimeoutDelta <= 0f;

        if (HasInputAuthority)
        {
            _localAnimatedSpeed = _animationBlend;
            _localAnimatedGrounded = Grounded;
            _localAnimatedFreeFall = freeFall;
        }

        if (HasStateAuthority)
        {
            AnimatedSpeed = _animationBlend;
            AnimatedGrounded = Grounded;
            AnimatedFreeFall = freeFall;
        }
    }

    private void UpdateAnimatorParameters()
    {
        if (!_animator)
            return;

        float speed = HasInputAuthority ? _localAnimatedSpeed : AnimatedSpeed;
        bool grounded = HasInputAuthority ? _localAnimatedGrounded : (bool)AnimatedGrounded;

        if (_playerCharacter != null && _playerCharacter.IsUsingUltimate)
        {
            grounded = false;
        }

        bool freeFall = HasInputAuthority ? _localAnimatedFreeFall : (bool)AnimatedFreeFall;

        _animator.SetFloat(_animIDSpeed, speed);
        _animator.SetFloat(_animIDMotionSpeed, 1f);
        _animator.SetBool(_animIDGrounded, grounded);
        _animator.SetBool(_animIDFreeFall, freeFall);
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");

        _animIDGrounded = Animator.StringToHash("Grounded");

        _animIDJump = Animator.StringToHash("Jump");

        _animIDJumpLand = Animator.StringToHash("Base Layer.JumpLand");

        _animIDFreeFall = Animator.StringToHash("FreeFall");

        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

        _animIDAttack = Animator.StringToHash("Attack");
    }
    private void PlayJumpAnimation()
    {
        Debug.Log("PLAY JUMP ANIMATION");
        _animator.SetBool(_animIDGrounded, false);
        _animator.SetBool(_animIDFreeFall, false);
        _animator.SetTrigger(_animIDJump);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayJumpAnimation()
    {
        PlayJumpAnimation();
    }

    private void PlayLandAnimation()
    {
        _animator.ResetTrigger("Jump");
        _animator.SetBool(_animIDGrounded, true);
        _animator.SetBool(_animIDFreeFall, false);
        _animator.CrossFade(_animIDJumpLand, 0f, 0, 0f);
        Debug.Log("PLAY LAND");
    }

    private void Attack(NetworkInputData input)
    {
        if (_playerCharacter != null &&
            _playerCharacter.IsDead)
        {
            return;
        }
        if (_playerCharacter != null && _playerCharacter.IsUsingUltimate)
        {
            return;
        }


        if (!input.Attack)
            return;

        if (_playerCharacter.IsDashing)
            return;

        if (HasStateAuthority)
        {
            _combat?.AttackInput();
        }
    }

    private void PlayAttackAnimation()
    {
        _animator.SetTrigger(_animIDAttack);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayAttackAnimation()
    {
        PlayAttackAnimation();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayLandAnimation()
    {
        PlayLandAnimation();
    }

    private void Dash(NetworkInputData input)
    {

        if (!input.Dash)
            return;

        if (_playerCharacter != null && _playerCharacter.IsUsingUltimate)
        {
            return;
        }

        if (_actionLock != null && !_actionLock.CanDash)
            return;

        if (_playerCharacter == null)
            return;

        if (_playerCharacter.IsDead)
            return;
        Debug.Log($"Dash Called {Runner.Tick}");
        _dash?.Dash();
    }

    private void ApplyClassDash()
    {
        if (_dash == null)
            return;

        float moveThisTick = _dash.GetMoveThisTick();

        if (moveThisTick <= 0f)
            return;

        Debug.Log($"Dash Move State:{HasStateAuthority} Input:{HasInputAuthority}");

        _controller.ForceMove(_dash.DashDirection * moveThisTick);
    }

    private void Skill(NetworkInputData input)
    {
        if (_playerCharacter != null && _playerCharacter.IsUsingUltimate)
        {
            return;
        }

        if (_actionLock != null && !_actionLock.CanSkill)
        {
            return;
        }

        if (input.SkillQ)
        {
            _skillQ?.UseQ();
        }

        if (input.SkillE)
        {
            _skillE?.UseE();
        }

        if (input.SkillR)
        {
            _skillR?.UseR();
        }
    }

    public void AddKnockback(Vector3 force)
    {
        if (!HasStateAuthority)
            return;

        if (_playerCharacter.IsDead)
            return;

        if (_playerCharacter.IsDashing)
            return;

        _knockbackVelocity += force;
    }
    private void ApplyKnockback()
    {
        if (_knockbackVelocity.sqrMagnitude < 0.001f)
            return;

        _controller.ForceMove(_knockbackVelocity * Runner.DeltaTime);

        _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, 12f * Runner.DeltaTime);
    }

    public void ResetControllerState()
    {
        _knockbackVelocity = Vector3.zero;

        _speed = 0f;
        _animationBlend = 0f;

        _airTime = 0f;

        _jumpCooldownTimer = 0f;

        _landingLockTimer = 0f;

        _fallTimeoutDelta = FallTimeout;

        Grounded = true;

        _wasGrounded = true;

        if (_animator != null)
        {
            _animator.Rebind();

            _animator.Update(0f);

            _animator.SetFloat(_animIDSpeed, 0f);

            _animator.SetBool(_animIDGrounded, true);

            _animator.SetBool(_animIDFreeFall, false);
        }
    }

    public void FootL() { }
    public void FootR() { }
}
