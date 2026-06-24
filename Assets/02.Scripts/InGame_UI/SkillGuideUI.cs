using UnityEngine;

public class SkillGuideUI : MonoBehaviour
{
    [SerializeField]
    private GameObject guidePanel;

    [SerializeField]
    private GameObject assassinGuide;

    [SerializeField]
    private GameObject mageGuide;

    [SerializeField]
    private GameObject bruteGuide;

    private bool _guideSet;
    private bool _hasShownFirstGuide;

    private void Start()
    {
        SetPanelVisible(false);
    }

    private void Update()
    {
        if (RoundManager.Instance == null)
        {
            SetPanelVisible(false);
            return;
        }

        RoundState state = RoundManager.Instance.CurrentState;

        if (state == RoundState.Playing ||
            state == RoundState.RoundEnd ||
            state == RoundState.GameEnd)
        {
            SetPanelVisible(false);
            return;
        }

        if (_hasShownFirstGuide)
        {
            return;
        }

        PlayerCharacter localPlayer = PlayerCharacter.Local;

        if (localPlayer == null)
        {
            SetPanelVisible(false);
            return;
        }

        if (localPlayer.ClassType == CharacterClassType.None)
        {
            SetPanelVisible(false);
            return;
        }

        if (!_guideSet)
        {
            ShowGuide(localPlayer.ClassType);
            _guideSet = true;
        }

        if (state == RoundState.CharacterSelect ||
            state == RoundState.Preparation)
        {
            SetPanelVisible(true);
        }
    }

    private void ShowGuide(CharacterClassType classType)
    {
        assassinGuide.SetActive(false);
        mageGuide.SetActive(false);
        bruteGuide.SetActive(false);

        switch (classType)
        {
            case CharacterClassType.Assassin:
                assassinGuide.SetActive(true);
                break;

            case CharacterClassType.Mage:
                mageGuide.SetActive(true);
                break;

            case CharacterClassType.Brute:
                bruteGuide.SetActive(true);
                break;
        }
    }

    private void SetPanelVisible(bool visible)
    {
        if (guidePanel == null)
            return;

        if (guidePanel.activeSelf == visible)
            return;

        guidePanel.SetActive(visible);

        if (!visible &&
            _guideSet &&
            RoundManager.Instance != null &&
            RoundManager.Instance.CurrentState == RoundState.Playing)
        {
            _hasShownFirstGuide = true;
        }
    }
}
