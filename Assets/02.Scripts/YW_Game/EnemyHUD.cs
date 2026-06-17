using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHUD : MonoBehaviour
{
    [SerializeField] private Image enemyHpBar;
    [SerializeField] private TMP_Text enemyHpText;

    [SerializeField]
    private float hideDelay = 10f;

    private float _lastHitTime;

    private GameObject _target;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;

        if (_target == null)
        {
            Hide();
            return;
        }

        PlayerCharacter player =
            _target.GetComponent<PlayerCharacter>();

        if (player != null && player.IsDead)
        {
            Hide();
            return;
        }

        if (Time.time - _lastHitTime >= hideDelay)
        {
            Hide();
        }
    }

    public void ShowTarget(GameObject target,float curHP,float maxHP)
    {
        if (maxHP <= 0f)
            return;

        _target = target;

        _lastHitTime = Time.time;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        float percent = curHP / maxHP;

        enemyHpBar.fillAmount = percent;

        enemyHpText.text = $"{percent * 100f:F1}%";
    }

    public void Hide()
    {
        _target = null;

        gameObject.SetActive(false);
    }
}