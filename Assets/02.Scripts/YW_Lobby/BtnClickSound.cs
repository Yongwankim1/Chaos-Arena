using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BtnClickSound : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] SoundEntry entry = new SoundEntry();
    Button clickBtn;
    TMP_Dropdown tmpDropdown;
    Toggle toggle;
    private void Awake()
    {
        if (clickBtn == null)
            clickBtn = GetComponent<Button>();

        if (tmpDropdown == null)
            tmpDropdown = GetComponent<TMP_Dropdown>();

        if (toggle == null)
            toggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        if (clickBtn != null)
            clickBtn.onClick.AddListener(ClickSoundPlay);
    }

    private void OnDisable()
    {
        if (clickBtn != null)
            clickBtn.onClick.RemoveListener(ClickSoundPlay);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (tmpDropdown != null || toggle != null)
        {
            ClickSoundPlay();
        }
    }

    private void ClickSoundPlay()
    {
        SoundManager.Instance.PlayUI(entry);
    }
}