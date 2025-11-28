#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class FindMissingScripts : MonoBehaviour
{
    [MenuItem("Tools/Find Missing Scripts in Scene")]
    static void FindMissingInCurrentScene()
    {
        GameObject[] go = GameObject.FindObjectsOfType<GameObject>();
        int missingCount = 0;

        foreach (GameObject g in go)
        {
            Component[] components = g.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogWarning($"Missing script found on: {g.name}", g);
                    missingCount++;
                }
            }
        }

        Debug.Log($"Search complete! Found {missingCount} GameObjects with missing scripts.");
    }
}

#endif