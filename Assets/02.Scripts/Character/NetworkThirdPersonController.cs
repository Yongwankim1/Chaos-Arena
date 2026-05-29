using Fusion;
using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CharacterController))]
public class NetworkThirdPersonController : NetworkBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 2f;
    public float SprintSpeed = 5.335f;
    public float RotationSmoothTime = 0.12f;
    public float SpeedChangeRate = 10f;

    [Header("Jump")]
    public float JumpHeight = 1.2f;
    public float Gravity = -15f;
    public float JumpTimeout = 0.5f;
    public float FallTimeout = 0.15f;

    [Header("Ground")]
    public bool Grounded;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;

    [Header("Camera")]
    public Transform PlayerCameraRoot;
    public float TopClamp = 70f;
    public float BottomClamp = -30f;

    private CharacterController _controller;
    private Animator _animator;

    private float _speed;
    private float _animationBlend;
    private float _targetRotation;
    private float _rotationVelocity;

    private float _verticalVelocity;
    private const float _terminalVelocity = -53f;

    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private float _yaw;
    private float _pitch;

    private GameObject _mainCamera;

    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    public override void Spawned()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;

        if (HasInputAuthority)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

            var cine = FindFirstObjectByType<CinemachineCamera>();
            if (cine != null)
                cine.Target.TrackingTarget = PlayerCameraRoot;

            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput<NetworkInputData>(out var input))
            return;

        GroundedCheck();
        CameraRotation(input.Look);
        JumpAndGravity(input);
        Move(input);
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(
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

    private void CameraRotation(Vector2 lookInput)
    {
        if (!HasInputAuthority)
            return;

        _yaw += lookInput.x;
        _pitch += lookInput.y;

        _pitch = Mathf.Clamp(_pitch, BottomClamp, TopClamp);

        PlayerCameraRoot.rotation =
            Quaternion.Euler(_pitch, _yaw, 0f);
    }

    private void Move(NetworkInputData input)
    {
        float targetSpeed = input.Sprint ? SprintSpeed : MoveSpeed;

        if (input.Move == Vector2.zero)
            targetSpeed = 0f;

        float inputMagnitude = Mathf.Clamp01(input.Move.magnitude);

        _speed = targetSpeed * inputMagnitude;
        _animationBlend = _speed;

        Vector3 inputDirection =
            new Vector3(input.Move.x, 0f, input.Move.y).normalized;

        if (input.Move != Vector2.zero)
        {
            _targetRotation =
                Mathf.Atan2(inputDirection.x, inputDirection.z) *
                Mathf.Rad2Deg +
                _mainCamera.transform.eulerAngles.y;

            float rotation =
                Mathf.SmoothDampAngle(
                    transform.eulerAngles.y,
                    _targetRotation,
                    ref _rotationVelocity,
                    RotationSmoothTime);

            transform.rotation =
                Quaternion.Euler(0f, rotation, 0f);
        }

        Vector3 targetDirection =
            Quaternion.Euler(0f, _targetRotation, 0f) *
            Vector3.forward;

        _controller.Move(
            targetDirection.normalized *
            (_speed * Runner.DeltaTime) +
            Vector3.up *
            _verticalVelocity *
            Runner.DeltaTime);

        _animator.SetFloat(_animIDSpeed, _animationBlend);
        _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
    }

    private void JumpAndGravity(NetworkInputData input)
    {
        if (Grounded)
        {
            _fallTimeoutDelta = FallTimeout;

            _animator.SetBool(_animIDJump, false);
            _animator.SetBool(_animIDFreeFall, false);

            if (_verticalVelocity < 0f)
                _verticalVelocity = -2f;

            if (input.Jump && _jumpTimeoutDelta <= 0f)
            {
                _verticalVelocity =
                    Mathf.Sqrt(JumpHeight * -2f * Gravity);

                _animator.SetBool(_animIDJump, true);
            }

            if (_jumpTimeoutDelta >= 0f)
                _jumpTimeoutDelta -= Runner.DeltaTime;
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
                _animator.SetBool(_animIDFreeFall, true);
            }
        }

        if (_verticalVelocity > _terminalVelocity)
        {
            _verticalVelocity +=
                Gravity * Runner.DeltaTime;
        }
    }
    private void Land()
    {
    }
}
