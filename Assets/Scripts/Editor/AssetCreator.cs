using Framework;
using UnityEngine;
using UnityEditor;

public class AssetCreator
{
    [MenuItem("Assets/Create/GameData/MechData")]
    public static void CreateMechData() => ScriptableObjectUtility.CreateAsset<MechData>();

    [MenuItem("Assets/Create/GameData/WeaponData")]
    public static void CreateWeaponData() => ScriptableObjectUtility.CreateAsset<WeaponData>();

    [MenuItem("Assets/Create/GameData/HazardData")]
    public static void CreateHazardData() => ScriptableObjectUtility.CreateAsset<HazardData>();
}