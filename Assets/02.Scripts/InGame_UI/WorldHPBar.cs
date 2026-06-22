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

    private bool _initialized;
    private bool _forceHidden;

    private float _refreshTimer;
    public void Initialize(PlayerCharacter owner)
    {
        _owner = owner;

        _cam = Camera.main;
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
        _forceHidden = !visible;

        if (canvas != null)
        {
            canvas.enabled = visible;
        }
    }

    private void LateUpdate()
    {
        if (!_initialized)
        {
            TryInitialize();
        }

        if (!_initialized)
            return;

        _refreshTimer += Time.deltaTime;

        if (_refreshTimer >= 0.1f)
        {
            _refreshTimer = 0f;

            RefreshHP();
        }

        if (_cam == null)
        {
            _cam = Camera.main;

            if (_cam == null)
                return;
        }

        transform.forward = _cam.transform.forward;

        delayFill.fillAmount =
            Mathf.Lerp(
                delayFill.fillAmount,
                _targetFill,
                Time.deltaTime * delaySpeed);

        UpdateVisible();
    }

    private void UpdateVisible()
    {
        if (_forceHidden)
        {
            canvas.enabled = false;

            return;
        }

        if (_owner == null)
            return;

        Vector3 viewport =
            _cam.WorldToViewportPoint(
                _owner.transform.position);

        bool visible =
            viewport.z > 0f &&
            viewport.x > 0f &&
            viewport.x < 1f &&
            viewport.y > 0f &&
            viewport.y < 1f;

        canvas.enabled = visible;
    }

    private void TryInitialize()
    {
        if (_initialized)
            return;

        if (_owner == null)
            return;

        if (PlayerCharacter.Local == null)
            return;

        if (PlayerCharacter.Local.Team == TeamType.None)
            return;

        if (_owner.Team == TeamType.None)
            return;

        _isEnemy =
            PlayerCharacter.Local.Team != _owner.Team;

        hpFill.sprite =
            _isEnemy
            ? enemyHpSprite
            : allyHpSprite;

        RefreshHP();

        _initialized = true;
    }
}