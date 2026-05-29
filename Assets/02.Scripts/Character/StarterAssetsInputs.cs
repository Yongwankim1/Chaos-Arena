using UnityEngine;
using UnityEngine.InputSystem;

public class StarterAssetsInputs : MonoBehaviour
{
    [Header("Input Values")]
    public Vector2 move;
    public Vector2 look;

    public bool jump;
    public bool sprint;

    [Header("Mouse Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        if (cursorInputForLook)
        {
            look = value.Get<Vector2>();
        }
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jump = true;
        }
    }

    public void ConsumeJump()
    {
        jump = false;
    }

    public void OnSprint(InputValue value)
    {
        sprint = value.isPressed;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        Cursor.lockState =
            cursorLocked
                ? CursorLockMode.Locked
                : CursorLockMode.None;
    }
}