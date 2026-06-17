using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("HP")]
    [SerializeField]
    private Image hpFill;

    [SerializeField]
    private TMP_Text hpText;

    [Header("MP")]
    [SerializeField]
    private Image mpFill;

    [SerializeField]
    private TMP_Text mpText;

    private PlayerCharacter _player;

    [SerializeField]
    private SkillSlotUI dashSlot;
    [SerializeField]
    private SkillSlotUI qSkillSlot;
    [SerializeField]
    private SkillSlotUI eSlot;
    [SerializeField]
    private SkillSlotUI rSlot;
    public void Initialize(PlayerCharacter player)
    {
        Debug.Log("HUD Initialize");
        _player = player;

        Refresh();
    }

    private void Update()
    {
        if (_player == null)
            return;

        Refresh();
    }

    private void Refresh()
    {
        float hpPercent = _player.CurrentHP / _player.MaxHP;

        hpFill.fillAmount = hpPercent;

        hpText.text = $"{_player.CurrentHP:0} / {_player.MaxHP:0}";

        float mpPercent = _player.CurrentMana / _player.MaxMana;

        mpFill.fillAmount = mpPercent;

        mpText.text = $"{_player.CurrentMana:0} / {_player.MaxMana:0}";

        RefreshDash();
        RefreshSkillQ();
        RefreshSkillE();
        RefreshSkillR();
    }

    private void RefreshDash()
    {
        AssassinDash dash = _player.GetComponent<AssassinDash>();

        if (dash == null)
            return;

        float remainTime = dash.DashCooldown.RemainingTime(dash.Runner) ?? 0f;

        dashSlot.Refresh(remainTime, dash.Cooldown);
    }
    private void RefreshSkillQ()
    {
        AssassinSkill skill = _player.GetComponent<AssassinSkill>();

        if (skill == null)
            return;

        float remainTime = skill.Cooldown.RemainingTime(skill.Runner) ?? 0f;

        qSkillSlot.Refresh(remainTime, skill.CooldownDuration);
    }
    private void RefreshSkillE()
    {
        AssassinStealth stealth =
            _player.GetComponent<AssassinStealth>();

        if (stealth == null)
            return;

        float remainTime =
            stealth.CooldownTimer.RemainingTime(
                stealth.Runner) ?? 0f;

        eSlot.Refresh(
            remainTime,
            stealth.CooldownDuration);
    }
    private void RefreshSkillR()
    {
        AssassinUltimate ultimate =
            _player.GetComponent<AssassinUltimate>();

        if (ultimate == null)
            return;

        float remainTime =
            ultimate.CooldownTimer.RemainingTime(
                ultimate.Runner) ?? 0f;

        rSlot.Refresh(
            remainTime,
            ultimate.CooldownDuration);

        if (ultimate.IsUltimate)
        {
            float remain =
                ultimate.RemainingDuration;

            rSlot.Refresh(
                remain,
                ultimate.Duration);
        }
    }
}