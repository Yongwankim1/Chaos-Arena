using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class UserNickNameChecker : MonoBehaviour
{
    [Header("유저 닉네임")]
    [SerializeField] private string nickName;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] GameObject createPlayerPanel;
    [SerializeField] Button checkBtn;
    [SerializeField] GameObject lobbyPanel;

    public string NickName => nickName;

    public event Action OnSetNickName;

    private void Start()
    {
        if (string.IsNullOrEmpty(UserDataManager.Instance.UserData.UserName))
        {
            OpenPanel();
        }
        else
        {
            lobbyPanel.SetActive(true);
            OnSetNickName?.Invoke();
        }

    }
    private void OpenPanel()
    {
        createPlayerPanel.SetActive(true);
    }
    public void SetNickName()
    {
        if (string.IsNullOrEmpty(inputField.text)) inputField.text = "Player" + Random.Range(1,10000).ToString();

        nickName = inputField.text;
        if (UserDataManager.Instance == null) return;

        UserDataManager.Instance.CreateUserData(nickName);
        createPlayerPanel.SetActive(false);

        lobbyPanel.SetActive(true);
        OnSetNickName?.Invoke();
    }
}
