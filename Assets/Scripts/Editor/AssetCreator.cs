using Framework;
using UnityEngine;
using UnityEditor;

public class AssetCreator
{
    [MenuItem("Assets/Create/GameData/UnitData")]
    public static void CreateUnitData()
    {
        ScriptableObjectUtility.CreateAsset<UnitData>();
    }

    [MenuItem("Assets/Create/GameData/WeaponData")]
    public static void CreateWeaponData()
    {
        ScriptableObjectUtility.CreateAsset<WeaponData>();
    }
}