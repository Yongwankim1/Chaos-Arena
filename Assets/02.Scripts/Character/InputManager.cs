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
    public bool SkillQ;
    public bool SkillE;
    public bool SkillR;

    public bool InputBlocked;

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
        if (InputBlocked)
            return;

        Move = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        if (InputBlocked)
            return;

        Look = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (InputBlocked)
            return;

        Jump = value.isPressed;
    }
    public bool ConsumeJump()
    {
        if (InputBlocked)
            return false;

        if (!Jump)
            return false;

        Jump = false;
        return true;
    }
    public void OnSprint(InputValue value)
    {
        if (InputBlocked)
            return;

        Sprint = value.isPressed;
    }

    public void OnAttack(InputValue value)
    {
        if (InputBlocked)
            return;

        Attack = value.isPressed;
    }
    public void OnDash(InputValue value)
    {
        if (InputBlocked)
            return;

        Dash = value.isPressed;
    }

    public bool ConsumeDash()
    {
        if (InputBlocked)
            return false;

        if (!Dash)
            return false;

        Dash = false;

        return true;
    }

    public bool ConsumeAttack()
    {
        if (InputBlocked)
            return false;

        if (!Attack)
            return false;

        Attack = false;

        return true;
    }
    public void OnSkillQ(InputValue value)
    {
        if (InputBlocked)
            return;

        SkillQ = value.isPressed;
    }
    public bool ConsumeSkillQ()
    {
        if (InputBlocked)
            return false;
        if (!SkillQ)
            return false;

        SkillQ = false;

        return true;
    }
    public void OnSkillE(InputValue value)
    {
        if (InputBlocked)
            return;

        SkillE = value.isPressed;
    }
    public bool ConsumeSkillE()
    {
        if (InputBlocked)
            return false;
        if (!SkillE)
            return false;

        SkillE = false;

        return true;
    }

    public void OnSkillR(InputValue value)
    {
        if (InputBlocked)
            return;

        SkillR = value.isPressed;
    }
    public bool ConsumeSkillR()
    {
        if (InputBlocked)
            return false;
        if (!SkillR)
            return false;

        SkillR = false;

        return true;
    }

    public void ClearAllInput()
    {
        Move = Vector2.zero;
        Look = Vector2.zero;

        Jump = false;
        Sprint = false;

        Attack = false;
        Dash = false;

        SkillQ = false;
        SkillE = false;
        SkillR = false;
    }
}