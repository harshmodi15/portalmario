#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ForceResearialize
{
    [MenuItem("Tools/Force Reserialize All Scenes")]
    public static void Reserialize()
    {
        AssetDatabase.ForceReserializeAssets(new[] { "Assets/Scenes/allyTutorial.unity" });
        UnityEngine.Debug.Log("Reserialization complete.");
    }
}
#endif