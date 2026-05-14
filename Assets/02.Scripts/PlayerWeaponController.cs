using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("무기 목록")]
    [SerializeField] private WeaponData[] weapons;
    [Header("Input (Input System")]
    [SerializeField] private InputAction equipWeapon1Action = new InputAction("EquipWeapon1", InputActionType.Button, "<Keyboard>/1");
    [SerializeField] private InputAction equipWeapon2Action = new InputAction("EquipWeapon2", InputActionType.Button, "<Keyboard>/2");
    [SerializeField] private InputAction equipWeapon3Action = new InputAction("EquipWeapon3", InputActionType.Button, "<Keyboard>/3");
    [SerializeField] private InputAction attackAction = new InputAction("Attack", InputActionType.Button, "<Mouse>/leftButton");
    [SerializeField] private InputAction reloadAction = new InputAction("Reload", InputActionType.Button, "<Keyboard>/r");

    private WeaponRunTime currentWeapon;
    private int currentWeaponIndex;
    private float nextAttackTime;

    private void OnEnable()
    {
        equipWeapon1Action.performed += OnEquipWeapon1Performed;
        equipWeapon2Action.performed += OnEquipWeapon2Performed;
        equipWeapon3Action.performed += OnEquipWeapon3Performed;
        attackAction.performed += OnAttackPerformed;
        reloadAction.performed += OnReloadPerformed;

        equipWeapon1Action.Enable();
        equipWeapon2Action.Enable();
        equipWeapon3Action.Enable();
        attackAction.Enable();
        reloadAction.Enable();
    }

    private void OnDisable()
    {
        equipWeapon1Action.Disable();
        equipWeapon2Action.Disable();
        equipWeapon3Action.Disable();
        attackAction.Disable();
        reloadAction.Disable();

        equipWeapon1Action.performed -= OnEquipWeapon1Performed;
        equipWeapon2Action.performed -= OnEquipWeapon2Performed;
        equipWeapon3Action.performed -= OnEquipWeapon3Performed;
        attackAction.performed -= OnAttackPerformed;
        reloadAction.performed -= OnReloadPerformed;
    }

    private void OnEquipWeapon1Performed(InputAction.CallbackContext context)
    {
        EquipWeapon(0);
    }
    private void OnEquipWeapon2Performed(InputAction.CallbackContext context)
    {
        EquipWeapon(1);
    }
    private void OnEquipWeapon3Performed(InputAction.CallbackContext context)
    {
        EquipWeapon(2);
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        TryAttack();
    }

    private void OnReloadPerformed(InputAction.CallbackContext context)
    {
        Reload();
    }

    private void EquipWeapon(int index)
    {
        if(index < 0 || index >= weapons.Length || weapons == null || weapons.Length == 0) return;
        if(currentWeaponIndex == index) return;

        
        currentWeaponIndex = index;
        currentWeapon = new WeaponRunTime(weapons[currentWeaponIndex]);
    }

    private void TryAttack()
    {
        if(currentWeapon == null) return;

        if(Time.time < nextAttackTime)
        {
            Debug.Log("공격 쿨타임 중입니다");
            return;
        }
        if(!currentWeapon.HasAmmo())
        {
            Debug.Log("탄약이 없습니다. R키로 재장전하세요");
            return;
        }

        nextAttackTime = Time.time + (1f/currentWeapon.data.attackRate);

        currentWeapon.ConsumeAmmo();

        int finalDamage = CalculateDamage();

    }
    private int CalculateDamage()
    {
        int damage = currentWeapon.data.damage;

        float randomValue = Random.value;
        if (randomValue <= currentWeapon.data.criticalChance)
        {
            damage = Mathf.RoundToInt(damage * currentWeapon.data.criticalMulitiplier);
            Debug.Log("치명타 발생!");
        }
        return damage;
    }
    private void Reload()
    {
        if (currentWeapon == null) return;

        currentWeapon.Reload();
    }
}
