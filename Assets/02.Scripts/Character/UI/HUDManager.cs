using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [SerializeField]
    private PlayerHUD playerHUD;

    private void Awake()
    {
        Instance = this;
    }

    public void BindPlayer(PlayerCharacter player)
    {
        Debug.Log($"BindPlayer : {player.name}");

        playerHUD.Initialize(player);
    }
}