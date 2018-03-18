using Framework;
using UnityEngine;
using UnityEditor;

public class AssetCreator
{
    [MenuItem("Assets/Create/GameData/UnitData")]
    public static void CreateStylesheetFont()
    {
        ScriptableObjectUtility.CreateAsset<UnitData>();
    }
}