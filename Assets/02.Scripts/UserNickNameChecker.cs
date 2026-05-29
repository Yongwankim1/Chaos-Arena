using TMPro;
using UnityEngine;

public class UserNickNameChecker : MonoBehaviour
{
    [Header("유저 닉네임")]
    [SerializeField] private string nickName;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] GameObject createPlayerPanel;
    public string NickName => nickName;

    private void Awake()
    {
        createPlayerPanel.SetActive(true);
    }


    public void SetNickName(string nickName)
    {
        if (string.IsNullOrEmpty(nickName)) nickName = "Player" + Random.Range(1,10000).ToString();

        this.nickName = nickName;
    }
}
