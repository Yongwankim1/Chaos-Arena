using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInfo : MonoBehaviour
{
    [SerializeField] TMP_Text nickName;
    [SerializeField] Toggle readyToggle;

    public void Init(string nickName,bool isReady = false)
    {
        this.nickName.text = nickName;
        readyToggle.isOn = isReady;
    }

    public void SetReady(bool isReady)
    {
        readyToggle.isOn = isReady;
    }
}
