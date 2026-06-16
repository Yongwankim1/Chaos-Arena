using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHUD : MonoBehaviour
{
    [SerializeField] Image enemyHpBar;
    [SerializeField] TMP_Text enemyHpText;
    [SerializeField] GameObject player;

    public void Init()
    {
        //TODO::스폰된 자신 프리팹의 공격클래스 받아서 구독
    }

    private void OnDisable()
    {
        //TODO:: 구독 해제
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
