using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class FirebaseManager : MonoBehaviour
{
    private string databaseURL = "https://portalmario-cs526-default-rtdb.firebaseio.com/";
    public static FirebaseManager instance;
    public bool allowLoggingInEditor = true; 

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Ensures this object persists across scenes
        }
        else
        {
            Destroy(gameObject); // Prevents duplicates
        }
    }

    public void SendDatabyPUT(string path, string json)
    {
        if (!allowLoggingInEditor && Application.platform != RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("Skipping Firebase Logging: Running in Unity Editor or Standalone.");
            return;
        }

        StartCoroutine(PutToDatabase(path, json));
    }
    
    public void SendDatabyPOST(string path, string json)
    {
        if (!allowLoggingInEditor && Application.platform != RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("Skipping Firebase Logging: Running in Unity Editor or Standalone.");
            return;
        }

        StartCoroutine(PostToDatabase(path, json));
    }

    IEnumerator PutToDatabase(string path, string json)
    {
        string fullURL = databaseURL + path + ".json";
        byte[] jsonToSend = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(fullURL, "PUT");
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Data successfully sent to Firebase!");
        }
        else
        {
            Debug.LogError("Error sending data: " + request.error);
        }
    }

    IEnumerator PostToDatabase(string path, string json)
    {
        string fullURL = databaseURL + path + ".json";
        byte[] jsonToSend = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(fullURL, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Data successfully sent to Firebase!");
        }
        else
        {
            Debug.LogError("Error sending data: " + request.error);
        }
    }

    public void LogLevelStart(int levelNumber)
    {
        string playerID = PlayerStats.playerID;
        int attemptNumber = PlayerStats.GetRetryCount(levelNumber);
        string path = $"GoldCompletion/{playerID}/level_{levelNumber}/attempt_{attemptNumber}";

        int deaths = PlayerStats.GetDeathCount(levelNumber);
        int retries = PlayerStats.GetRetryCount(levelNumber) - 1;

        string json = $"{{\"completionTime\": \"{Mathf.RoundToInt(Time.timeSinceLevelLoad)}\", \"deaths\": {deaths}, \"retries\": {retries}, \"completed\": false}}";
        SendDatabyPUT(path, json);
    }

    public void UpdateDeathCount(int levelNumber)
    {
        string playerID = PlayerStats.playerID;
        int attemptNumber = PlayerStats.GetRetryCount(levelNumber);
        string path = $"GoldCompletion/{playerID}/level_{levelNumber}/attempt_{attemptNumber}";

        int deaths = PlayerStats.GetDeathCount(levelNumber);
        int retries = PlayerStats.GetRetryCount(levelNumber) - 1;

        string json = $"{{\"completionTime\": \"{Mathf.RoundToInt(Time.timeSinceLevelLoad)}\", \"deaths\": {deaths}, \"retries\": {retries}, \"completed\": false}}";
        SendDatabyPUT(path, json);
    }

    public void UpdateRetryCount(int levelNumber)
    {
        string playerID = PlayerStats.playerID;
        int attemptNumber = PlayerStats.GetRetryCount(levelNumber);
        string path = $"GoldCompletion/{playerID}/level_{levelNumber}/attempt_{attemptNumber}";

        int deaths = PlayerStats.GetDeathCount(levelNumber);
        int retries = PlayerStats.GetRetryCount(levelNumber) - 1;

        string json = $"{{\"completionTime\": \"{Mathf.RoundToInt(Time.timeSinceLevelLoad)}\", \"deaths\": {deaths}, \"retries\": {retries}, \"completed\": false}}";
        SendDatabyPUT(path, json);
    }

    public void UpdateLevelCompletion(int levelNumber, float completionTime)
    {
        string playerID = PlayerStats.playerID;
        int attemptNumber = PlayerStats.GetRetryCount(levelNumber);
        string path = $"GoldCompletion/{playerID}/level_{levelNumber}/attempt_{attemptNumber}";

        int deaths = PlayerStats.GetDeathCount(levelNumber);
        int retries = PlayerStats.GetRetryCount(levelNumber) - 1;

        string json = $"{{\"completionTime\": \"{Mathf.RoundToInt(completionTime)}\", \"deaths\": {deaths}, \"retries\": {retries}, \"completed\": true}}";
        SendDatabyPUT(path, json);
    }

    public void LogTestDataByPOST(string key, object data, int levelNumber)
    {
        string playerID = PlayerStats.playerID;
        int attemptNumber = PlayerStats.GetRetryCount(levelNumber);
        string path = $"Gold/{playerID}/level_{levelNumber}/attempt_{attemptNumber}/{key}";
        string json = JsonUtility.ToJson(data);
        SendDatabyPOST(path, json);
}

    public void LogEnemyKill(string reason, Vector2 position, int level, string enemyType)
    {
        EnemyKillData data = new EnemyKillData(reason, position, Time.timeSinceLevelLoad, enemyType);
        LogTestDataByPOST("enemy_kills", data, level);
    }

    public void LogKeyPress(Vector2 position, float time, int level)
    {
        KeyPressData data = new KeyPressData(position, time);
        LogTestDataByPOST("keyPresses", data, level);
    }

    public void LogPortalTraversal(string objectType, Vector2 from, Vector2 to, float velocity,int level)
    {
        PortalTraversalData data = new PortalTraversalData(objectType, from, to, velocity, Time.timeSinceLevelLoad);
        LogTestDataByPOST("portal_usage", data, level);
    }
}