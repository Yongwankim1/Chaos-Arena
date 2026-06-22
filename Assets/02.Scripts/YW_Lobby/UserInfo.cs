using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInfo : MonoBehaviour
{
    [SerializeField] TMP_Text nickName;
    [SerializeField] Toggle readyToggle;
    [SerializeField] private TMP_Dropdown teamDropdown;
    private RoomPlayerData _playerData;

    private int _lastBlueCount = -1;
    private int _lastRedCount = -1;

    private void Awake()
    {
        teamDropdown.onValueChanged.AddListener(OnTeamChanged);
    }

    private void OnDestroy()
    {
        teamDropdown.onValueChanged.RemoveListener(OnTeamChanged);
    }

    private void Update()
    {
        int blueCount = RoomUserListUI.GetBlueCount();
        int redCount = RoomUserListUI.GetRedCount();

        if (blueCount == _lastBlueCount &&
            redCount == _lastRedCount)
        {
            return;
        }

        _lastBlueCount = blueCount;
        _lastRedCount = redCount;

        RefreshTeamDropdown();
    }

    public void Init(string nickName, bool isReady = false)
    {
        this.nickName.text = nickName;
        readyToggle.isOn = isReady;
    }

    public void Init(RoomPlayerData playerData)
    {
        _playerData = playerData;

        nickName.text = playerData.NickName.ToString();

        readyToggle.isOn = playerData.IsReady;

        teamDropdown.SetValueWithoutNotify((int)playerData.TeamSelect);

        teamDropdown.interactable = playerData.Object.HasInputAuthority;

        RefreshTeamDropdown();
    }

    public void SetReady(bool isReady)
    {
        readyToggle.isOn = isReady;
    }

    private void OnTeamChanged(int value)
    {
        if (_playerData == null)
        {
            return;
        }

        TeamSelectType selectedTeam = (TeamSelectType)value;

        if (!CanSelectTeam(selectedTeam))
        {
            teamDropdown.SetValueWithoutNotify((int)_playerData.TeamSelect);

            return;
        }

        _playerData.RPC_SetTeam(selectedTeam);
    }

    private void RefreshTeamDropdown()
    {
        int blueCount = RoomUserListUI.GetBlueCount();
        int redCount = RoomUserListUI.GetRedCount();
        int maxCount = RoomUserListUI.MaxTeamCount;

        teamDropdown.options[1].text = $"Blue ({blueCount}/{maxCount})";

        teamDropdown.options[2].text = $"Red ({redCount}/{maxCount})";
        teamDropdown.RefreshShownValue();
    }
    private bool CanSelectTeam(TeamSelectType team)
    {
        if (team == TeamSelectType.Random)
        {
            return true;
        }

        int blueCount = RoomUserListUI.GetBlueCount();

        int redCount = RoomUserListUI.GetRedCount();

        int maxCount = RoomUserListUI.MaxTeamCount;

        if (team == TeamSelectType.Blue)
        {
            if (_playerData.TeamSelect != TeamSelectType.Blue)
            {
                return blueCount < maxCount;
            }

            return true;
        }

        if (team == TeamSelectType.Red)
        {
            if (_playerData.TeamSelect != TeamSelectType.Red)
            {
                return redCount < maxCount;
            }

            return true;
        }

        return true;
    }
}
