using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyReturnAfterDelay : MonoBehaviour
{
    private bool _started;

    public static void StartReturn(float delay, int sceneIndex)
    {
        LobbyReturnAfterDelay existing = FindFirstObjectByType<LobbyReturnAfterDelay>();

        if (existing != null)
        {
            existing.Begin(delay, sceneIndex);

            return;
        }

        GameObject obj = new GameObject(nameof(LobbyReturnAfterDelay));

        DontDestroyOnLoad(obj);

        obj.AddComponent<LobbyReturnAfterDelay>().Begin(delay, sceneIndex);
    }

    private void Begin(float delay, int sceneIndex)
    {
        if (_started)
        {
            return;
        }

        _started = true;

        StartCoroutine(ReturnRoutine(delay, sceneIndex));
    }

    private IEnumerator ReturnRoutine(float delay, int sceneIndex)
    {
        yield return new WaitForSecondsRealtime(delay);

        _ = ShutdownAndLoadLobby(sceneIndex);
    }

    private async System.Threading.Tasks.Task ShutdownAndLoadLobby(int sceneIndex)
    {
        NetworkRunner runner = FindFirstObjectByType<NetworkRunner>();

        if (runner != null)
        {
            await runner.Shutdown(
                destroyGameObject: true,
                shutdownReason: ShutdownReason.Ok,
                forceShutdownProcedure: true
            );
        }

        SceneManager.LoadScene(sceneIndex);

        Destroy(gameObject);
    }
}
