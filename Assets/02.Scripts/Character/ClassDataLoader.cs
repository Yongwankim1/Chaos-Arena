using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

public static class ClassDataLoader
{
    public static async Task<ClassData> LoadClassData(
        CharacterClassType classType)
    {
        return await Addressables
            .LoadAssetAsync<ClassData>(
                classType.ToString())
            .Task;
    }
}