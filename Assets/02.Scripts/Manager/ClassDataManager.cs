using System.Collections.Generic;
using UnityEngine;

public static class ClassDataManager
{
    private static Dictionary<CharacterClassType, ClassData>
        _classDatas =
            new Dictionary<CharacterClassType, ClassData>();

    public static void AddData(
        CharacterClassType classType,
        ClassData data)
    {
        _classDatas[classType] = data;
    }

    public static ClassData GetData(
        CharacterClassType classType)
    {
        return _classDatas[classType];
    }

    public static bool IsLoaded(
        CharacterClassType classType)
    {
        return _classDatas.ContainsKey(classType);
    }
}