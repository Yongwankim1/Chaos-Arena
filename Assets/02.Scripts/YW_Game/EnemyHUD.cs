using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHUD : MonoBehaviour
{
    [SerializeField] Image enemyHpBar;
    [SerializeField] TMP_Text enemyHpText;
    private CharacterCombat combat;
    public void Init(PlayerCharacter player)
    {
        combat = player.GetComponent<CharacterCombat>();
        // combat에 타겟 HP 변경 이벤트를 만든 뒤 여기서 구독
        // combat.OnTargetHPChanged += TargetHPChange;
    }

    private void OnDisable()
    {
        //TODO:: 구독 해제
        if (combat != null)
        {
            // combat.OnTargetHPChanged -= TargetHPChange;
        }
    }

    private void TargetHPChange(GameObject target)
    {
        if(!target.TryGetComponent<IHasHealth>(out var hasHealth))
        {
            return;
        }

        hasHealth.GetHPInfo(out float curHP, out float maxHP);

        if (maxHP <= 0f)
            return;
        float percent = (curHP / maxHP);
        enemyHpBar.fillAmount = percent;
        percent *= 100f;
        enemyHpText.text = $"{percent:F1}%";
    }
}
