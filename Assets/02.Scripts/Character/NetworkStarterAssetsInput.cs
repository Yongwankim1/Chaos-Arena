using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkStarterAssetsInput : MonoBehaviour
{
    public static NetworkStarterAssetsInput Local { get; private set; }

    public Vector2 Move;
    public Vector2 Look;

    public bool Jump;
    public bool Sprint;

    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        if (Local == this)
        {
            LockCursor();
        }
    }

    private void OnDisable()
    {
        if (Local == this)
        {
            Local = null;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && Local == this)
        {
            LockCursor();
        }
    }

    public void SetInputAuthority(bool hasInputAuthority)
    {
        ClearInput();

        if (_playerInput == null)
        {
            _playerInput = GetComponent<PlayerInput>();
        }

        if (_playerInput != null)
        {
            _playerInput.enabled = hasInputAuthority;
        }

        enabled = hasInputAuthority;

        if (hasInputAuthority)
        {
            Local = this;
            LockCursor();
        }
        else if (Local == this)
        {
            Local = null;
        }
    }

    public void OnMove(InputValue value)
    {
        Move = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        Look = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        Jump = value.isPressed;
    }

    public void OnSprint(InputValue value)
    {
        Sprint = value.isPressed;
    }

    public bool ConsumeJump()
    {
        if (!Jump)
            return false;

        Jump = false;
        return true;
    }

    private void ClearInput()
    {
        Move = Vector2.zero;
        Look = Vector2.zero;
        Jump = false;
        Sprint = false;
    }

    private static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
