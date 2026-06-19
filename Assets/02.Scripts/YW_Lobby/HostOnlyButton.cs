using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostOnlyButton : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropDownBtn;
    private NetworkRunner runner;

    private void Start()
    {
        runner = FindFirstObjectByType<NetworkRunner>();

        bool isHost =
            runner != null &&
            (runner.IsServer || runner.IsSharedModeMasterClient);

        dropDownBtn.interactable = isHost;
    }
}