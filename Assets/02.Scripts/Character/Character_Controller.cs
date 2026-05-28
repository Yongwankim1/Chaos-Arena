using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
[RequireComponent(typeof(PlayerInput))]
#endif
public class Character_Controller : MonoBehaviour
{
    public enum CharacterState
    {
        Idle,
        Move,
        Jump,
        Fall,
        Land
    }

    [Header("Move")]
    public float MoveSpeed = 5.0f;
    public float SprintSpeed = 12.0f;
    public float RotationSmoothTime = 0.12f;
    public float SpeedChangeRate = 10.0f;

    [Header("Jump")]
    public float JumpHeight = 1.2f;
    public float Gravity = -15.0f;
    public float TerminalVelocity = 20.0f;

    [Header("Ground")]
    public bool Grounded = false;

    [Header("Camera")]
    public GameObject CinemachineCameraTarget;
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;
    public bool LockCameraPosition = false;

    private CharacterState _currentState;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    private float _speed;
    private float _animationBlend;
    private float _targetRotation;
    private float _rotationVelocity;
    private float _verticalVelocity;

    // Animator IDs
    private int _animIDMoving;
    private int _animIDAnimationSpeed;
    private int _animIDVelocity;
    private int _animIDJumping;
    private int _animIDTrigger;
    private int _animIDTriggerNumber;

    private Animator _animator;
    private CharacterController _controller;
    private StarterAssetsInputs _input;
    private PlayerInput _playerInput;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private bool _landed;

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return _playerInput.currentControlScheme == "KeyboardMouse";
#else
            return false;
#endif
        }
    }

    private void Awake()
    {
        _mainCamera =
            GameObject.FindGameObjectWithTag("MainCamera");
    }

    private void Start()
    {
        _cinemachineTargetYaw =
            CinemachineCameraTarget
            .transform.rotation.eulerAngles.y;

        _animator =
            GetComponent<Animator>();

        _controller =
            GetComponent<CharacterController>();

        _input =
            GetComponent<StarterAssetsInputs>();

        _playerInput =
            GetComponent<PlayerInput>();

        AssignAnimationIDs();

        ChangeState(CharacterState.Idle);
    }

    private void Update()
    {
        GroundedCheck();

        JumpAndGravity();

        Move();

        UpdateAnimator();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void AssignAnimationIDs()
    {
        _animIDMoving =
            Animator.StringToHash("Moving");

        _animIDAnimationSpeed =
            Animator.StringToHash("Animation Speed");

        _animIDVelocity =
            Animator.StringToHash("Velocity");

        _animIDJumping =
            Animator.StringToHash("Jumping");

        _animIDTrigger =
            Animator.StringToHash("Trigger");

        _animIDTriggerNumber =
            Animator.StringToHash("Trigger Number");
    }

    private void SetAnimatorTrigger(int number)
    {
        _animator.SetInteger(
            _animIDTriggerNumber,
            number
        );

        _animator.SetTrigger(
            _animIDTrigger
        );
    }

    private void ChangeState(CharacterState newState)
    {
        if (_currentState == newState)
            return;

        _currentState = newState;

        switch (_currentState)
        {
            case CharacterState.Idle:

                _animator.SetBool(
                    _animIDMoving,
                    false
                );

                _animator.SetInteger(
                    _animIDJumping,
                    0
                );

                break;

            case CharacterState.Move:

                _animator.SetBool(
                    _animIDMoving,
                    true
                );

                _animator.SetInteger(
                    _animIDJumping,
                    0
                );

                break;

            case CharacterState.Jump:

                _landed = false;

                _animator.SetInteger(
                    _animIDJumping,
                    1
                );

                SetAnimatorTrigger(1);

                break;

            case CharacterState.Fall:

                _animator.SetInteger(
                    _animIDJumping,
                    2
                );

                SetAnimatorTrigger(1);

                break;

            case CharacterState.Land:

                _animator.SetInteger(
                    _animIDJumping,
                    0
                );

                SetAnimatorTrigger(1);

                break;
        }

        Debug.Log("STATE : " + _currentState);
    }

    private void GroundedCheck()
    {
        Grounded = _controller.isGrounded;
    }

    private void CameraRotation()
    {
        if (_input.look.sqrMagnitude >= _threshold
            && !LockCameraPosition)
        {
            float deltaTimeMultiplier =
                IsCurrentDeviceMouse
                ? 1.0f
                : Time.deltaTime;

            _cinemachineTargetYaw +=
                _input.look.x * deltaTimeMultiplier;

            _cinemachineTargetPitch +=
                _input.look.y * deltaTimeMultiplier;
        }

        _cinemachineTargetPitch =
            Mathf.Clamp(
                _cinemachineTargetPitch,
                BottomClamp,
                TopClamp
            );

        CinemachineCameraTarget.transform.rotation =
            Quaternion.Euler(
                _cinemachineTargetPitch,
                _cinemachineTargetYaw,
                0.0f
            );
    }

    private void Move()
    {
        float targetSpeed =
            _input.sprint
            ? SprintSpeed
            : MoveSpeed;

        if (_input.move == Vector2.zero)
        {
            targetSpeed = 0.0f;
        }

        float currentHorizontalSpeed =
            new Vector3(
                _controller.velocity.x,
                0.0f,
                _controller.velocity.z
            ).magnitude;

        float speedOffset = 0.1f;

        float inputMagnitude =
            _input.analogMovement
            ? _input.move.magnitude
            : 1f;

        if (currentHorizontalSpeed
            < targetSpeed - speedOffset
            ||
            currentHorizontalSpeed
            > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(
                currentHorizontalSpeed,
                targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate
            );

            _speed =
                Mathf.Round(_speed * 1000f)
                / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(
            _animationBlend,
            targetSpeed,
            Time.deltaTime * SpeedChangeRate
        );

        Vector3 inputDirection =
            new Vector3(
                _input.move.x,
                0.0f,
                _input.move.y
            ).normalized;

        if (_input.move != Vector2.zero)
        {
            _targetRotation =
                Mathf.Atan2(
                    inputDirection.x,
                    inputDirection.z
                ) * Mathf.Rad2Deg
                + _mainCamera.transform.eulerAngles.y;

            float rotation =
                Mathf.SmoothDampAngle(
                    transform.eulerAngles.y,
                    _targetRotation,
                    ref _rotationVelocity,
                    RotationSmoothTime
                );

            transform.rotation =
                Quaternion.Euler(
                    0.0f,
                    rotation,
                    0.0f
                );
        }

        Vector3 targetDirection =
            Quaternion.Euler(
                0.0f,
                _targetRotation,
                0.0f
            ) * Vector3.forward;

        _controller.Move(
            targetDirection.normalized
            * (_speed * Time.deltaTime)
            +
            new Vector3(
                0.0f,
                _verticalVelocity,
                0.0f
            ) * Time.deltaTime
        );

        if (Grounded)
        {
            if (_currentState == CharacterState.Idle
                || _currentState == CharacterState.Move)
            {
                if (_input.move != Vector2.zero)
                {
                    ChangeState(CharacterState.Move);
                }
                else
                {
                    ChangeState(CharacterState.Idle);
                }
            }
        }
    }

    private void JumpAndGravity()
    {
        switch (_currentState)
        {
            case CharacterState.Idle:
            case CharacterState.Move:

                if (!Grounded)
                {
                    ChangeState(CharacterState.Fall);
                    return;
                }

                if (_verticalVelocity < 0)
                {
                    _verticalVelocity = -2f;
                }

                if (_input.jump)
                {
                    _verticalVelocity =
                        Mathf.Sqrt(
                            JumpHeight
                            * -2f
                            * Gravity
                        );

                    ChangeState(CharacterState.Jump);

                    _input.jump = false;

                    return;
                }

                break;

            case CharacterState.Jump:

                if (_verticalVelocity <= 0)
                {
                    ChangeState(CharacterState.Fall);
                    return;
                }

                break;

            case CharacterState.Fall:

                if (Grounded && !_landed)
                {
                    _landed = true;

                    _verticalVelocity = -2f;

                    ChangeState(CharacterState.Land);

                    return;
                }

                break;

            case CharacterState.Land:

                if (_input.move != Vector2.zero)
                {
                    ChangeState(CharacterState.Move);
                }
                else
                {
                    ChangeState(CharacterState.Idle);
                }

                break;
        }

        if (_verticalVelocity > -TerminalVelocity)
        {
            _verticalVelocity +=
                Gravity * Time.deltaTime;
        }
    }

    private void UpdateAnimator()
    {
        if (_animator == null)
            return;

        float currentVelocity =
            transform.InverseTransformDirection(
                _controller.velocity
            ).z;

        _animator.SetFloat(
            _animIDVelocity,
            currentVelocity
        );

        _animator.SetFloat(
            _animIDAnimationSpeed,
            AnimationCurve(currentVelocity)
        );
    }

    private float AnimationCurve(float velocity)
    {
        float normalized =
            Mathf.Abs(velocity) / SprintSpeed;

        return Mathf.Lerp(
            0.8f,
            1.4f,
            normalized
        );
    }

    public void FootL()
    {
    }

    public void FootR()
    {
    }

    public void Land()
    {
    }
}