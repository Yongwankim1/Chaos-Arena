using Fusion;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    public Button okButton;
    public int lobbySceneIndex = 0;
    private bool isReturningToLobby;

    [Networked] public NetworkBool IsGameOver { get; set; }

    public void Start()
    {
        if (okButton != null)
            okButton.onClick.AddListener(OnClickReturnToLobby);
    }

    public override void Spawned()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
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
        await ReturnToLobby(true);
    }

    private async Task ReturnToLobby(bool notifyClients)
    {
        if (isReturningToLobby)
            return;

        isReturningToLobby = true;

        if (okButton != null)
            okButton.interactable = false;

        int sceneToLoad = lobbySceneIndex;
        NetworkRunner currentRunner = Runner;

        if (currentRunner != null)
        {
            if (notifyClients && currentRunner.IsServer)
            {
                RPC_RequestClientsReturnToLobby();
                await Task.Delay(300);
            }

            await currentRunner.Shutdown(
                destroyGameObject: true,
                shutdownReason: ShutdownReason.Ok,
                forceShutdownProcedure: true
            );
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_RequestClientsReturnToLobby()
    {
        if (Runner != null && Runner.IsServer)
            return;

        _ = ReturnToLobby(false);
    }
}
