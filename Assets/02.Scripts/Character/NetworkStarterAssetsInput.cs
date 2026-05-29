using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkStarterAssetsInput : MonoBehaviour
{
    public static NetworkStarterAssetsInput Local;

    public Vector2 Move;
    public Vector2 Look;

    public bool Jump;
    public bool Sprint;

    private NetworkObject _networkObject;

    private void Awake()
    {
        _networkObject = GetComponent<NetworkObject>();
    }

    private void Update()
    {
        if (_networkObject != null &&
            _networkObject.HasInputAuthority)
        {
            Local = this;
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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
}