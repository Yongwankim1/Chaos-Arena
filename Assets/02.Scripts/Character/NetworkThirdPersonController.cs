using Fusion;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(NetworkCharacterController))]
public class NetworkThirdPersonController : NetworkBehaviour
{
    [Header("Player")]
    public float MoveSpeed = 2.0f;
    public float SprintSpeed = 5.335f;
    public float RotationSmoothTime = 0.01f;
    public float SpeedChangeRate = 10.0f;

    [Header("Jump")]
    public float JumpCooldown = 0.25f;
    public float FallTimeout = 0.02f;

    private bool _wasGrounded; 
    private bool _justLanded;

    private bool _jumpTriggered;

    private float _landingLockTimer;

    private float _jumpCooldownTimer;
    private float _fallTimeoutDelta;

    [Header("Ground")]
    public bool Grounded = true;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;

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


    private Animator _animator;
    private NetworkCharacterController _controller;

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

    [Networked]
    private float AnimatedSpeed { get; set; }

    [Networked]
    private NetworkBool AnimatedGrounded { get; set; }

    [Networked]
    private NetworkBool AnimatedFreeFall { get; set; }

    private float _localAnimatedSpeed;
    private bool _localAnimatedGrounded = true;
    private bool _localAnimatedFreeFall;

    public override void Spawned()
    {

        _animator = GetComponent<Animator>();
        _controller = GetComponent<NetworkCharacterController>();

        AssignAnimationIDs();

        //_jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
        _jumpCooldownTimer = 0f;
        _jumpTriggered = false;

        _wasGrounded = true;
        _landingLockTimer = 0f;

        if (HasInputAuthority)
        {
            GetComponent<NetworkStarterAssetsInput>()
                ?.RegisterAsLocal();

            var cam = FindObjectOfType<Unity.Cinemachine.CinemachineCamera>();

            if (cam != null)
            {
                cam.Target.TrackingTarget = PlayerCameraRoot;
                //cam.Target.LookAtTarget = PlayerCameraRoot;
            }

            _cinemachineTargetYaw = transform.eulerAngles.y;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData input))
            return;
        _lastInput = input;
        // CameraRotation(input);
        GroundedCheck();
        JumpAndGravity(input);
        Move(input);
        GroundedCheck();
        CaptureAnimationState();
        UpdateAnimatorParameters();
    }

    private void LateUpdate()
    {
        if (HasInputAuthority)
        {
            CameraRotation(_lastInput);
        }
    }

    private void GroundedCheck()
    {
        bool groundedNow = _controller.Grounded;

        _justLanded = false;

        if (!_wasGrounded && groundedNow)
        {
            _landingLockTimer = 0.2f;
            _justLanded = true;

            if (HasStateAuthority)
            {
                RPC_PlayLandAnimation();
            }
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
                BottomClamp,
                TopClamp);

        PlayerCameraRoot.rotation =
            Quaternion.Euler(
                _cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw,
                0f);

    }

    private void Move(NetworkInputData input)
    {
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
        float inputMagnitude = input.Move == Vector2.zero ? 1f : 1f;

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
    private void JumpAndGravity(NetworkInputData input)
    {
        if (_jumpCooldownTimer > 0f)
            _jumpCooldownTimer -= Runner.DeltaTime;

        if (_landingLockTimer > 0f)
            _landingLockTimer -= Runner.DeltaTime;

        if (Grounded)
        {
            _fallTimeoutDelta = FallTimeout;

            _jumpTriggered = false;

            if (input.Jump && _jumpCooldownTimer <= 0f && _landingLockTimer <= 0f)
            {
                float previousVerticalVelocity = _controller.Velocity.y;
                _controller.Jump();

                bool jumpStarted = _controller.Velocity.y > previousVerticalVelocity;

                if (jumpStarted)
                {
                    _controller.Grounded = false;
                    Grounded = false;

                    if (HasStateAuthority)
                    {
                        RPC_PlayJumpAnimation();
                    }

                    _jumpTriggered = true;

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
        bool freeFall = !Grounded && _fallTimeoutDelta <= 0f;

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
        bool freeFall = HasInputAuthority ? _localAnimatedFreeFall : (bool)AnimatedFreeFall;

        _animator.SetFloat(_animIDSpeed, speed);
        _animator.SetFloat(_animIDMotionSpeed, 1f);
        _animator.SetBool(_animIDGrounded, grounded);
        _animator.SetBool(_animIDFreeFall, freeFall);
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed =
            Animator.StringToHash("Speed");

        _animIDGrounded =
            Animator.StringToHash("Grounded");

        _animIDJump =
            Animator.StringToHash("Jump");

        _animIDJumpLand =
            Animator.StringToHash("Base Layer.JumpLand");

        _animIDFreeFall =
            Animator.StringToHash("FreeFall");

        _animIDMotionSpeed =
            Animator.StringToHash("MotionSpeed");
    }
    private void PlayJumpAnimation()
    {
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
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayLandAnimation()
    {
        PlayLandAnimation();
    }

    private void Land()
    {
    }
}
