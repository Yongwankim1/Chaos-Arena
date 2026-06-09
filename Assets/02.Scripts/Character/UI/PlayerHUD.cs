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

    public void Initialize(
        PlayerCharacter player)
    {
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
        Debug.Log(_player.MaxHP);
        float hpPercent =
            _player.CurrentHP /
            _player.MaxHP;

        hpFill.fillAmount =
            hpPercent;

        hpText.text =
            $"{_player.CurrentHP:0} / {_player.MaxHP:0}";

        float mpPercent =
            _player.CurrentMana /
            _player.MaxMana;

        mpFill.fillAmount =
            mpPercent;

        mpText.text =
            $"{_player.CurrentMana:0} / {_player.MaxMana:0}";
    }
}