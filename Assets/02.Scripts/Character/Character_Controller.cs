using UnityEngine;
using Fusion;
using Unity.Cinemachine;

[RequireComponent(typeof(CharacterController))]
public class Character_Controller : NetworkBehaviour
{
    [Header("Move")]
    public float MoveSpeed = 5f;
    public float SprintSpeed = 8f;
    public float RotationSmoothTime = 0.05f;

    [Header("Jump")]
    public float JumpHeight = 1.2f;
    public float Gravity = -35f;

    [Header("Camera")]
    public GameObject CinemachineCameraTarget;

    private CharacterController _controller;
    private Animator _animator;
    private NetworkMecanimAnimator _networkAnimator;

    private Transform _cameraTransform;

    private float _verticalVelocity;
    private float _targetRotation;
    private float _rotationVelocity;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    private bool _grounded;

    private int _animIDMoving;
    private int _animIDVelocity;
    private int _animIDJumping;
    private int _animIDFalling;

    public override void Spawned()
    {
        _controller =
            GetComponent<CharacterController>();

        _animator =
            GetComponent<Animator>();

        _networkAnimator =
            GetComponent<NetworkMecanimAnimator>();

        AssignAnimationIDs();

        ConnectCamera();

        _cinemachineTargetYaw =
            CinemachineCameraTarget
            .transform.eulerAngles.y;

        _cameraTransform =
            Camera.main.transform;
    }

    private void AssignAnimationIDs()
    {
        _animIDMoving =
            Animator.StringToHash("Moving");

        _animIDVelocity =
            Animator.StringToHash("Velocity");

        _animIDJumping =
            Animator.StringToHash("Jumping");

        _animIDFalling =
            Animator.StringToHash("Falling");
    }

    private void ConnectCamera()
    {
        if (!HasStateAuthority)
            return;

        Invoke(nameof(SetCamera), 0.2f);
    }

    private void SetCamera()
    {
        CinemachineCamera camera =
            FindFirstObjectByType<CinemachineCamera>();

        if (camera == null)
            return;

        camera.Follow =
            CinemachineCameraTarget.transform;

        camera.LookAt =
            CinemachineCameraTarget.transform;
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData data))
            return;

        GroundedCheck();

        Move(data);

        Jump(data);

        GravityUpdate();

        CameraRotation(data);
    }

    private void GroundedCheck()
    {
        _grounded =
            _controller.isGrounded;
    }

    private void Move(NetworkInputData data)
    {
        Vector2 moveInput =
            data.Move;

        float targetSpeed =
            data.Buttons.IsSet(
                (int)EInputButtons.Sprint)
            ? SprintSpeed
            : MoveSpeed;

        if (moveInput == Vector2.zero)
        {
            targetSpeed = 0f;
        }

        Vector3 inputDirection =
            new Vector3(
                moveInput.x,
                0f,
                moveInput.y
            ).normalized;

        if (moveInput != Vector2.zero)
        {
            _targetRotation =
                Mathf.Atan2(
                    inputDirection.x,
                    inputDirection.z
                ) * Mathf.Rad2Deg
                + _cameraTransform.eulerAngles.y;

            float rotation =
                Mathf.SmoothDampAngle(
                    transform.eulerAngles.y,
                    _targetRotation,
                    ref _rotationVelocity,
                    RotationSmoothTime
                );

            transform.rotation =
                Quaternion.Euler(
                    0f,
                    rotation,
                    0f
                );
        }

        Vector3 targetDirection =
            Quaternion.Euler(
                0f,
                _targetRotation,
                0f
            ) * Vector3.forward;

        _controller.Move(
            (
                targetDirection.normalized
                * targetSpeed
                +
                Vector3.up
                * _verticalVelocity
            )
            * Runner.DeltaTime
        );

        _animator.SetBool(
            _animIDMoving,
            moveInput != Vector2.zero
        );

        _animator.SetFloat(
            _animIDVelocity,
            targetSpeed
        );
    }

    private void Jump(NetworkInputData data)
    {
        bool jumpPressed =
            data.Buttons.IsSet(
                (int)EInputButtons.Jump);

        if (_grounded
            &&
            _verticalVelocity < 0)
        {
            _verticalVelocity = -2f;

            _animator.SetBool(
                _animIDFalling,
                false
            );
        }

        if (jumpPressed
            &&
            _grounded)
        {
            _verticalVelocity =
                Mathf.Sqrt(
                    JumpHeight
                    * -2f
                    * Gravity
                );

            _grounded = false;

            _animator.SetBool(
                _animIDJumping,
                true
            );

            _animator.SetBool(
                _animIDFalling,
                false
            );
        }

        if (!_grounded
            &&
            _verticalVelocity < 0)
        {
            _animator.SetBool(
                _animIDJumping,
                false
            );

            _animator.SetBool(
                _animIDFalling,
                true
            );
        }

        if (_grounded
            &&
            _verticalVelocity <= 0)
        {
            _animator.SetBool(
                _animIDJumping,
                false
            );

            _animator.SetBool(
                _animIDFalling,
                false
            );
        }
    }

    private void GravityUpdate()
    {
        if (_verticalVelocity > -50f)
        {
            _verticalVelocity +=
                Gravity
                * Runner.DeltaTime;
        }
    }

    private void CameraRotation(
        NetworkInputData data)
    {
        if (!HasStateAuthority)
            return;

        Vector2 look =
            data.Look;

        _cinemachineTargetYaw +=
            look.x * 0.1f;

        _cinemachineTargetPitch -=
            look.y * 0.1f;

        _cinemachineTargetPitch =
            Mathf.Clamp(
                _cinemachineTargetPitch,
                -30f,
                70f
            );

        CinemachineCameraTarget
            .transform.rotation =
            Quaternion.Euler(
                _cinemachineTargetPitch,
                _cinemachineTargetYaw,
                0f
            );
    }

    public void Land()
    {
    }

    public void FootL()
    {
    }

    public void FootR()
    {
    }
}