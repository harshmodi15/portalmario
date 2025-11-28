using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Level2CompleteUI : MonoBehaviour
{
    public TextMeshProUGUI timeText2;

    void Start()
    {
        float finalTime = PlayerPrefs.GetFloat("FinalTime", 0f);
        int minutes = Mathf.FloorToInt(finalTime / 60);
        int seconds = Mathf.FloorToInt(finalTime % 60);
        timeText2.text = "Victory? Maybe.\nSpeedrun-Worthy? Doubt It.\n\nTime: " + string.Format("{0:00}:{1:00}", minutes, seconds);
        Debug.Log("Loaded Final Time: " + finalTime);
    }

    public void RetryLevel()
    {
        PlayerStats.levelNumber = 2;
        PlayerStats.IncreaseRetryCount(2);
        PlayerStats.ResetDeathCount(2);
        SceneManager.LoadScene("lvl2");        // SceneManager.LoadScene("lvl2");
        if (FirebaseManager.instance != null)
        {
            FirebaseManager.instance.UpdateRetryCount(2);
        }
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}