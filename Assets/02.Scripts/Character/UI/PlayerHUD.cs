using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private Image hpFill;
    [SerializeField] private Image hpDelayFill;
    [SerializeField] private TMP_Text hpText;

    [Header("MP")]
    [SerializeField] private Image mpFill;
    [SerializeField] private Image mpDelayFill;
    [SerializeField] private TMP_Text mpText;

    [Header("Delay")]
    [SerializeField] private float hpDelaySpeed = 1.5f;
    [SerializeField] private float mpDelaySpeed = 1.5f;

    private float _targetHpFill;
    private float _targetMpFill;

    private bool _needHpAnimation;
    private bool _needMpAnimation;

    private PlayerCharacter _player;

    [SerializeField] private SkillSlotUI dashSlot;
    [SerializeField] private SkillSlotUI qSkillSlot;
    [SerializeField] private SkillSlotUI eSlot;
    [SerializeField] private SkillSlotUI rSlot;
    [SerializeField] private CrosshairTargetIndicator crosshair;

    private float _lastHP = -9999;
    private float _lastMP = -9999;

    private IDash _dash;
    private ISkillQ _skillQ;
    private ISkillE _skillE;
    private ISkillR _skillR;

    public void Initialize(PlayerCharacter player)
    {
        Debug.Log("HUD Initialize");

        _player = player;

        _dash = player.GetComponent<IDash>();
        _skillQ = player.GetComponent<ISkillQ>();
        _skillE = player.GetComponent<ISkillE>();
        _skillR = player.GetComponent<ISkillR>();
        SetupSkillIcons();

        if (crosshair == null)
        {
            crosshair = CrosshairTargetIndicator.CreateDefault(transform);
        }

        crosshair?.Initialize(player);
    }

    private void Update()
    {
        if (_player == null)
            return;

        if (_player.MaxHP <= 0)
            return;

        if (_player.MaxMana <= 0)
            return;

        if (!Mathf.Approximately(_lastHP, _player.CurrentHP))
        {
            _lastHP = _player.CurrentHP;
            RefreshHP();
        }

        if (!Mathf.Approximately(_lastMP, _player.CurrentMana))
        {
            _lastMP = _player.CurrentMana;
            RefreshMP();
        }

        RefreshSkills();

        UpdateDelayedBars();
    }

    public void RefreshHP()
    {
        if (_player.MaxHP <= 0)
            return;

        float hpPercent = _player.CurrentHP / _player.MaxHP;

        hpFill.fillAmount = hpPercent;

        hpText.text = $"{_player.CurrentHP:0} / {_player.MaxHP:0}";

        if (hpPercent < hpDelayFill.fillAmount)
        {
            _targetHpFill = hpPercent;
            _needHpAnimation = true;
        }
        else
        {
            hpDelayFill.fillAmount = hpPercent;
            _targetHpFill = hpPercent;
        }
    }

    public void RefreshMP()
    {
        if (_player.MaxMana <= 0)
            return;

        float mpPercent = _player.CurrentMana / _player.MaxMana;

        mpFill.fillAmount = mpPercent;

        mpText.text = $"{_player.CurrentMana:0} / {_player.MaxMana:0}";

        if (mpPercent < mpDelayFill.fillAmount)
        {
            _targetMpFill = mpPercent;
            _needMpAnimation = true;
        }
        else
        {
            mpDelayFill.fillAmount = mpPercent;
            _targetMpFill = mpPercent;
        }
    }

    private void UpdateDelayedBars()
    {
        if (_needHpAnimation)
        {
            hpDelayFill.fillAmount = Mathf.MoveTowards(
                hpDelayFill.fillAmount,
                _targetHpFill,
                hpDelaySpeed * Time.deltaTime);

            if (Mathf.Abs(hpDelayFill.fillAmount - _targetHpFill) < 0.001f)
            {
                hpDelayFill.fillAmount = _targetHpFill;
                _needHpAnimation = false;
            }
        }

        if (_needMpAnimation)
        {
            mpDelayFill.fillAmount = Mathf.MoveTowards(
                mpDelayFill.fillAmount,
                _targetMpFill,
                mpDelaySpeed * Time.deltaTime);

            if (Mathf.Abs(mpDelayFill.fillAmount - _targetMpFill) < 0.001f)
            {
                mpDelayFill.fillAmount = _targetMpFill;
                _needMpAnimation = false;
            }
        }
    }

    private void RefreshSkills()
    {
        RefreshSkill(_dash, dashSlot);
        RefreshSkill(_skillQ, qSkillSlot);
        RefreshSkill(_skillE, eSlot);
        RefreshSkill(_skillR, rSlot);
    }

    private void RefreshSkill(object skill, SkillSlotUI slot)
    {
        if (skill == null)
        {
            slot.gameObject.SetActive(false);
            return;
        }

        slot.gameObject.SetActive(true);

        ISkillCooldown cooldown = skill as ISkillCooldown;

        if (cooldown == null)
            return;

        float remain = cooldown.CooldownTimer.RemainingTime(_player.Runner) ?? 0f;

        slot.Refresh(remain, cooldown.CooldownDuration);

        IActiveSkill active = skill as IActiveSkill;

        if (active != null && active.IsActive)
        {
            slot.Refresh(active.RemainingDuration, active.Duration);
        }
    }
    private void SetupSkillIcons()
    { 
        ClassData data = ClassDataManager.GetData(_player.ClassType); 
        if (data == null) return;
        qSkillSlot.SetIcon(data.SkillIcons.Q); 
        eSlot.SetIcon(data.SkillIcons.E); rSlot.SetIcon(data.SkillIcons.R);
        dashSlot.SetIcon(data.SkillIcons.Dash);
    }
}
