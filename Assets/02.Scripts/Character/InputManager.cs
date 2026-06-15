using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public Vector2 Move;
    public Vector2 Look;

    public bool Jump;
    public bool Sprint;

    public bool Attack;
    public bool Dash;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
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
    public bool ConsumeJump()
    {
        if (!Jump)
            return false;

        Jump = false;
        return true;
    }
    public void OnSprint(InputValue value)
    {
        Sprint = value.isPressed;
    }

    public void OnAttack(InputValue value)
    {
        Attack = value.isPressed;
    }
    public void OnDash(InputValue value)
    {
        Dash = value.isPressed;
    }

    public bool ConsumeDash()
    {
        if (!Dash)
            return false;

        Dash = false;

        return true;
    }

    public bool ConsumeAttack()
    {
        if (!Attack)
            return false;

        Attack = false;

        return true;
    }
}