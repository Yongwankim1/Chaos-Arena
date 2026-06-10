using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBarUI : MonoBehaviour
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
        if (hpText == null || hpBar == null) return;
        float value = (float) currentHP / maxHP;
        hpBar.fillAmount = value;
        hpText.text = $"{currentHP}/{maxHP}";
    }
}
