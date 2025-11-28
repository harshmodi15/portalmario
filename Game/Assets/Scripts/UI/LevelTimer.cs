using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float timeElapsed;
    private bool isRunning = false;

    void Start()
    {
        timeElapsed = 0f;
        isRunning = true;
    }

    void Update()
    {
        if (isRunning)
        {
            timeElapsed += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timeElapsed / 60);
        int seconds = Mathf.FloorToInt(timeElapsed % 60);
        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    public float GetTime()
    {
        return timeElapsed;
    }

    public void StopTimer()
    {
        isRunning = false;
        PlayerPrefs.SetFloat("FinalTime", timeElapsed);
        PlayerPrefs.Save();
        Debug.Log("Final Time: " + timeElapsed); // Check if time is stopping correctly

    }
}