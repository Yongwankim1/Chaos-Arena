using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [SerializeField]
    private PlayerHUD playerHUD;
    [SerializeField]
    private EnemyHUD enemyHUD;

    public EnemyHUD EnemyHUD => enemyHUD;

    private void Awake()
    {
        Instance = this;
    }

    public void BindPlayer(PlayerCharacter player)
    {
        playerHUD.Initialize(player);
    }
}