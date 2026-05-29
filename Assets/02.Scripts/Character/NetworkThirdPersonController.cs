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
    public float JumpTimeout = 0.5f;
    public float FallTimeout = 0.15f;
    [Networked]
    private int JumpCounter { get; set; }

    private int _lastJumpCounter;

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

    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private Animator _animator;
    private NetworkCharacterController _controller;

    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private const float _threshold = 0.01f;
    [SerializeField]
    float lookMultiplier = 0.2f;
    private NetworkInputData _lastInput;
    public override void Spawned()
    {
        Debug.Log(
        $"Spawned {Object.Id} " +
        $"Input:{Object.InputAuthority} " +
        $"State:{Object.StateAuthority}");

        _animator = GetComponent<Animator>();
        _controller = GetComponent<NetworkCharacterController>();

        AssignAnimationIDs();

        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;

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
        Move(input);
        GroundedCheck();
        JumpAndGravity(input);
    }

    private void LateUpdate()
    {
        if (HasInputAuthority)
        {
            CameraRotation(_lastInput);
        }
    }

    public override void Render()
    {
        if (JumpCounter != _lastJumpCounter)
        {
            _lastJumpCounter = JumpCounter;

            _animator.SetTrigger("Jump");
        }
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition =
            new Vector3(
                transform.position.x,
                transform.position.y - GroundedOffset,
                transform.position.z);

        Grounded = Physics.CheckSphere(
            spherePosition,
            GroundedRadius,
            GroundLayers,
            QueryTriggerInteraction.Ignore);

        _animator.SetBool(_animIDGrounded, Grounded);
    }

    private void CameraRotation(NetworkInputData input)
    {
        if (!HasInputAuthority)
            return;

        if (input.Look.sqrMagnitude >= _threshold)
        {
            _cinemachineTargetYaw += input.Look.x * lookMultiplier;
            _cinemachineTargetPitch -= input.Look.y * lookMultiplier;
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
    Debug.Log(
    $"Obj:{Object.Id} " +
    $"InputAuth:{Object.InputAuthority} " +
    $"StateAuth:{Object.StateAuthority} " +
    $"HasInput:{HasInputAuthority}");

        Debug.Log(
    $"Input:{HasInputAuthority} " +
    $"State:{HasStateAuthority}"
);

        Debug.Log(
    $"Runner:{Runner.Mode} " +
    $"Input:{HasInputAuthority} " +
    $"State:{HasStateAuthority}"
);
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
        float inputMagnitude = input.Move == Vector2.zero ? 0f : 1f;

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

        if (_animator)
        {
            _animator.SetFloat(
                _animIDSpeed,
                _animationBlend);

            _animator.SetFloat(
                _animIDMotionSpeed,
                inputMagnitude);
        }
    }

    private void JumpAndGravity(NetworkInputData input)
    {
        if (Grounded)
        {
            _fallTimeoutDelta = FallTimeout;

            _animator.SetBool(
                _animIDFreeFall,
                false);

            if (input.Jump &&
                _jumpTimeoutDelta <= 0f)
            {
                _controller.Jump();

                // 점프 애니메이션 이벤트 전송
                JumpCounter++;
            }

            if (_jumpTimeoutDelta >= 0f)
            {
                _jumpTimeoutDelta -= Runner.DeltaTime;
            }
        }
        else
        {
            _jumpTimeoutDelta = JumpTimeout;

            if (_fallTimeoutDelta >= 0f)
            {
                _fallTimeoutDelta -= Runner.DeltaTime;
            }
            else
            {
                _animator.SetBool(
                    _animIDFreeFall,
                    true);
            }
        }
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed =
            Animator.StringToHash("Speed");

        _animIDGrounded =
            Animator.StringToHash("Grounded");

        _animIDJump =
            Animator.StringToHash("Jump");

        _animIDFreeFall =
            Animator.StringToHash("FreeFall");

        _animIDMotionSpeed =
            Animator.StringToHash("MotionSpeed");
    }
    private void Land()
    {
    }
}