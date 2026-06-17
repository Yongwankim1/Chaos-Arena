using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHUD : MonoBehaviour
{
    [SerializeField] Image enemyHpBar;
    [SerializeField] TMP_Text enemyHpText;
    [SerializeField] private CharacterCombat combat;
    
    public void Init(PlayerCharacter player)
    {
        if (!player.TryGetComponent<CharacterCombat>(out combat)) return;
        // combat에 타겟 HP 변경 이벤트를 만든 뒤 여기서 구독
        combat.OnAttackTargetChanged += TargetHPChange;
    }

    private void OnDisable()
    {
        //TODO:: 구독 해제
        if (combat != null) combat.OnAttackTargetChanged -= TargetHPChange;
    }

    private void TargetHPChange(float curHP, float maxHP)
    {
        Debug.Log("이벤트 호출");

        if (maxHP <= 0f)
            return;

        float percent = curHP / maxHP;
        enemyHpBar.fillAmount = percent;
        enemyHpText.text = $"{percent * 100f:F1}%";
    }
}
