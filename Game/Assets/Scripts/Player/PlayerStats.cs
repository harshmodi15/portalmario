using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static string playerID;
    public static int levelNumber = 0;
    public static Dictionary<int, int> retryCounts = new Dictionary<int, int>();
    public static Dictionary<int, int> deathCounts = new Dictionary<int, int>();
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (string.IsNullOrEmpty(playerID))
        {
            playerID = System.Guid.NewGuid().ToString();  // Generate Unique Player ID
        }
    }

    public static void ResetDeathCount(int level)
    {
        deathCounts[level] = 0;
    }

    public static void IncreaseDeathCount(int level)
    {
        if (!deathCounts.ContainsKey(level))
            deathCounts[level] = 0;

        deathCounts[level]++;
    }

    public static void IncreaseRetryCount(int level)
    {
        if (!retryCounts.ContainsKey(level))
            retryCounts[level] = 0;

        retryCounts[level]++;
    }

    public static int GetRetryCount(int level)
    {
        return retryCounts.ContainsKey(level) ? retryCounts[level] : 0;
    }

    public static int GetDeathCount(int level)
    {
        return deathCounts.ContainsKey(level) ? deathCounts[level] : 0;
    }
}