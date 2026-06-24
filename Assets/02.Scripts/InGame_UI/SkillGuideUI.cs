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

    private void Start()
    {
        guidePanel.SetActive(false);
    }

    private void Update()
    {
        if (RoundManager.Instance == null)
            return;

        if (PlayerCharacter.Local == null)
            return;

        if (!_guideSet && PlayerCharacter.Local.ClassType != CharacterClassType.None)
        {
            ShowGuide(PlayerCharacter.Local.ClassType);

            _guideSet = true;
        }

        // 캐릭터 선택 후 대기시간
        if (RoundManager.Instance.CurrentRound == 1 &&
            RoundManager.Instance.CurrentState == RoundState.Preparation)
        {
            if (!guidePanel.activeSelf)
            {
                guidePanel.SetActive(true);
            }
        }
        else
        {
            if (guidePanel.activeSelf)
            {
                guidePanel.SetActive(false);
            }
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
}