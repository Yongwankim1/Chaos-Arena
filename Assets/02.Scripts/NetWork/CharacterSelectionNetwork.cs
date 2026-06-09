//using Fusion;
//using UnityEngine;

//public class CharacterSelectionNetwork : NetworkBehaviour
//{
//    public static CharacterSelectionNetwork Instance;

//    private void Awake()
//    {
//        Instance = this;
//    }

//    public void SelectCharacter(
//        CharacterClassType classType)
//    {
//        RPC_SelectCharacter(classType);
//    }

//    [Rpc(
//     RpcSources.All,
//     RpcTargets.StateAuthority)]
//    private void RPC_SelectCharacter(
//     CharacterClassType classType,
//     RpcInfo info = default)
//    {
//        Debug.Log(
//            $"RPC Received : {info.Source.PlayerId} -> {classType}");

//        GameBootstrap bootstrap =
//            FindFirstObjectByType<GameBootstrap>();

//        bootstrap.RegisterCharacterSelection(
//            info.Source,
//            classType);
//        bootstrap.SpawnSelectedCharacter(
//    info.Source);
//    }
//}