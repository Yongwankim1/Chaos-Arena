using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    public Button okButton;
    public int lobbySceneIndex = 0;

    [Networked] public NetworkBool IsGameOver { get; set; }

    public void Start()
    {
        Instance = this;
        okButton.onClick.AddListener(OnClickReturnToLobby);
    }

    public override void Spawned()
    {
        if(Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || IsGameOver) return;
    }


    private async void OnClickReturnToLobby()
    {
        if (okButton != null) okButton.interactable = false;

        int sceneToLoad = lobbySceneIndex;
        NetworkRunner currentRunner = Runner;

        if (currentRunner != null)
        {
            await currentRunner.Shutdown();

            if(currentRunner != null && currentRunner.gameObject != null)
            {
                Destroy(currentRunner.gameObject);
            }
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}
