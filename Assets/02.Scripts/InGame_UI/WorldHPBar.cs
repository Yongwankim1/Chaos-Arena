using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldHPBar : MonoBehaviour
{
    [Header("HP")]
    [SerializeField]
    private Image hpFill;

    [SerializeField]
    private Image delayFill;
    [SerializeField]
    private TMP_Text hpText;
    [Header("Sprite")]
    [SerializeField]
    private Sprite allyHpSprite;

    [SerializeField]
    private Sprite enemyHpSprite;

    [Header("Follow")]
    [SerializeField]
    private Canvas canvas;

    [Header("Delay")]
    [SerializeField]
    private float delaySpeed = 3f;

    private Camera _cam;

    private PlayerCharacter _owner;

    private float _targetFill;

    private bool _isEnemy;

    public void Initialize(
        PlayerCharacter owner,
        bool isEnemy)
    {
        _owner = owner;

        _isEnemy = isEnemy;

        _cam = Camera.main;

        hpFill.sprite =isEnemy? enemyHpSprite: allyHpSprite;

        RefreshHP();
    }

    public void RefreshHP()
    {
        if (_owner == null)
            return;

        float hpPercent =
            _owner.CurrentHP /
            _owner.MaxHP;

        _targetFill = hpPercent;

        hpFill.fillAmount = hpPercent;

        if (hpText != null)
        {
            hpText.text =$"{Mathf.CeilToInt(_owner.CurrentHP)} / {Mathf.CeilToInt(_owner.MaxHP)}";
        }
    }

    public void SetVisible(bool visible)
    {
        if (canvas == null)
            return;

        canvas.enabled = visible;
    }

    private void LateUpdate()
    {
        if (_cam == null)
        {
            _cam = Camera.main;

            if (_cam == null)
                return;
        }

        transform.forward =_cam.transform.forward;

        delayFill.fillAmount =
            Mathf.Lerp(delayFill.fillAmount,_targetFill,Time.deltaTime * delaySpeed);

        UpdateVisible();
    }

    private void UpdateVisible()
    {
        if (_owner == null)
            return;

        Vector3 viewport = _cam.WorldToViewportPoint(_owner.transform.position);

        bool visible =
            viewport.z > 0f &&
            viewport.x > 0f &&
            viewport.x < 1f &&
            viewport.y > 0f &&
            viewport.y < 1f;

        canvas.enabled = visible;
    }
}