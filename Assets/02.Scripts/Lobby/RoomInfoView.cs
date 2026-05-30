using TMPro;
using UnityEngine;

public class RoomInfoView : MonoBehaviour
{
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text hostNameText;

    public void SetRoomInfo(string roomName, string hostName)
    {
        roomNameText.text = "방: " + roomName;
        hostNameText.text = "호스트: " + hostName;
    }

    public void Clear()
    {
        roomNameText.text = "";
        hostNameText.text = "";
    }
}