using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBarUI : NetworkBehaviour
{
    [SerializeField] EnemyHP enemyHP;
    [SerializeField] Image hpBar;
    [SerializeField] TMP_Text hpText;
    private void Awake()
    {
        if(hpBar == null) hpBar = GetComponentInChildren<Image>();
        if(hpText == null) hpText = GetComponentInChildren<TMP_Text>();
        if(enemyHP == null) enemyHP = GetComponentInParent<EnemyHP>();
    }
    private void OnEnable()
    {
        if(enemyHP != null)
        {
            enemyHP.OnHPChange += HPBarUpdate;
        }
    }

    private void OnDisable()
    {
        if (enemyHP != null)
        {
            enemyHP.OnHPChange -= HPBarUpdate;
        }
    }

    private void HPBarUpdate(int maxHP, int currentHP)
    {
        if (Object != null && !Object.HasStateAuthority)
            return;
        if (hpText == null || hpBar == null) return;

        RPC_HpBarUpdate(maxHP, currentHP);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HpBarUpdate(int maxHP, int currentHP)
    {
        float value = (float)currentHP / maxHP;
        hpBar.fillAmount = value;
        hpText.text = $"{currentHP}/{maxHP}";
    }
}
