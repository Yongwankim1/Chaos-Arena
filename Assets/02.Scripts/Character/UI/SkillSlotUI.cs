using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [SerializeField]
    private Image cooldownFill;

    [SerializeField]
    private TMP_Text cooldownText;

    [SerializeField]
    private Image icon;

    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
        icon.SetNativeSize();
        RectTransform rect = icon.rectTransform;

        rect.sizeDelta /= 1.5f;
    }

    public void Refresh(float remainTime, float maxTime)
    {
        if (remainTime <= 0f)
        {
            cooldownFill.fillAmount = 0f;
            cooldownText.text = "";
            return;
        }

        cooldownFill.fillAmount = remainTime / maxTime;

        cooldownText.text = Mathf.CeilToInt(remainTime).ToString();
    }
}