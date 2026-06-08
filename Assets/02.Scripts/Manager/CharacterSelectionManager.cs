using Fusion;
using UnityEngine;

public class CharacterSelectManager : NetworkBehaviour
{
    public static CharacterSelectManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void SelectAssassin()
    {
        FindLocalData().RPC_SelectCharacter(CharacterClassType.Assassin);
    }

    public void SelectMage()
    {
        FindLocalData().RPC_SelectCharacter(CharacterClassType.Mage);
    }

    public void Ready()
    {
        FindLocalData().RPC_SetReady();
    }

    private PlayerSelectionData FindLocalData()
    {
        return FindObjectOfType<PlayerSelectionData>();
    }
}