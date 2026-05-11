using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] PlayerInputReader inputReader;
    [SerializeField] Animator m_animator;
    [SerializeField] int AttackCount = 0;

    [SerializeField] float defaultDamage = 0;
    [SerializeField] float currentDamage = 0;
    [SerializeField] float equipDamage = 0;
    public float Dmaage => currentDamage;

    int attackHash = -999;

    private void Awake()
    {
        Initialize();
        attackHash = Animator.StringToHash("Attack");
    }
    void Initialize()
    {
        if(inputReader == null)
            inputReader = GetComponent<PlayerInputReader>();
        if(m_animator == null)
            m_animator = GetComponent<Animator>();
        currentDamage = defaultDamage;
    }
    public void EquipWeapon(float amount)
    {
        equipDamage = amount;
    }

    private void Update()
    {
        AttackRotation();
        DefalutAttack();
    }
    void AttackRotation()
    {
        if (!inputReader.IsAttackPerformedThisFrame) return;
        Transform carmeraTransform = Camera.main.transform;
        Vector3 forward = carmeraTransform.forward;
        forward.y = 0f;

        forward.Normalize();
        Vector3 moveDir = forward;
        Quaternion rot = Quaternion.LookRotation(moveDir);

        transform.rotation = rot;
    }
    void DefalutAttack()
    {
        if (!inputReader.IsAttackPerformedThisFrame) return;
        m_animator.SetTrigger(attackHash);
        //m_animator.SetInteger("AttackCount",AttackCount);
    }
    public void LastAttack()
    {
        currentDamage = (defaultDamage + equipDamage) * 1.3f;
    }
    public void StatAttack()
    {
        //weapon.StartAttack();
    }
    public void EndAttack()
    {
        //weapon.EndAttack();
        currentDamage = (defaultDamage + equipDamage);
    }

}
