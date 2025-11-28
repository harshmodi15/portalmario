using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Level1CompleteUI : MonoBehaviour
{
    public TextMeshProUGUI timeText1;

    void Start()
    {
        float finalTime = PlayerPrefs.GetFloat("FinalTime", 0f);
        int minutes = Mathf.FloorToInt(finalTime / 60);
        int seconds = Mathf.FloorToInt(finalTime % 60);
        timeText1.text = "Not Bad!! Not Bad!!\nBut Can You Survive The Next Level?\n\nTime: " + string.Format("{0:00}:{1:00}", minutes, seconds);
        Debug.Log("Loaded Final Time: " + finalTime);
    }

    public void RetryLevel()
    {
        PlayerStats.levelNumber = 1;
        PlayerStats.IncreaseRetryCount(1);
        PlayerStats.ResetDeathCount(1);
        SceneManager.LoadScene("lvl1");
        if (FirebaseManager.instance != null)
        {
            FirebaseManager.instance.UpdateRetryCount(1);
        }
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void NextLevel()
    {
        PlayerStats.levelNumber = 2;
        PlayerStats.IncreaseRetryCount(2);
        PlayerStats.ResetDeathCount(2);
        SceneManager.LoadScene("lvl2");
        if (FirebaseManager.instance != null)
        {
            FirebaseManager.instance.LogLevelStart(2);
        }
    }
}