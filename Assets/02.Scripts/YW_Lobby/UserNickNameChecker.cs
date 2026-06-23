using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class UserNickNameChecker : MonoBehaviour
{
    [Header("유저 닉네임")]
    [SerializeField] private string nickName;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] GameObject createPlayerPanel;
    [SerializeField] NetFadeInFadeOut fade;
    [SerializeField] Button checkBtn;
    [SerializeField] GameObject lobbyPanel;
    [SerializeField] private float fadeOutDuration = 0.5f;

    public string NickName => nickName;

    public event Action OnSetNickName;

    private bool isSettingNickName;

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
        if (isSettingNickName)
            return;

        StartCoroutine(SetNickNameRoutine());
    }

    private IEnumerator SetNickNameRoutine()
    {
        isSettingNickName = true;

        if (inputField != null)
        {
            inputField.DeactivateInputField();
        }

        yield return null;
        yield return WaitForImeCommitAndSubmitRelease();

        string inputText =
            inputField != null
                ? inputField.text
                : string.Empty;

        if (string.IsNullOrEmpty(inputText))
        {
            inputText = "Player" + Random.Range(1, 10000);

            if (inputField != null)
            {
                inputField.text = inputText;
            }
        }

        nickName = inputText;

        if (UserDataManager.Instance == null)
        {
            isSettingNickName = false;
            yield break;
        }

        UserDataManager.Instance.CreateUserData(nickName);

        lobbyPanel.SetActive(true);
        createPlayerPanel.SetActive(false);

        if (fade == null)
        {
            FinishSetNickName();
            yield break;
        }

        fade.LocalFadeOut(fadeOutDuration, FinishSetNickName);
    }

    private void FinishSetNickName()
    {
        isSettingNickName = false;
        OnSetNickName?.Invoke();
    }

    private IEnumerator WaitForImeCommitAndSubmitRelease()
    {
        while (!string.IsNullOrEmpty(Input.compositionString))
        {
            yield return null;
        }

        while (Keyboard.current != null &&
               (Keyboard.current.enterKey.isPressed ||
                Keyboard.current.numpadEnterKey.isPressed))
        {
            yield return null;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
