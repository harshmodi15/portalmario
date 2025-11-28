using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TutorialCompleteUI : MonoBehaviour
{
    public TextMeshProUGUI timeText0;

    void Start()
    {
        float finalTime = PlayerPrefs.GetFloat("FinalTime", 0f);
        int minutes = Mathf.FloorToInt(finalTime / 60);
        int seconds = Mathf.FloorToInt(finalTime % 60);
        timeText0.text = "Nice Moves!\nYou’ve Mastered The Basics.\n\nTime: " + string.Format("{0:00}:{1:00}", minutes, seconds);
        Debug.Log("Loaded Final Time: " + finalTime);
    }

    public void RetryLevel()
    {
        PlayerStats.levelNumber = -1;
        PlayerStats.IncreaseRetryCount(-1);
        PlayerStats.ResetDeathCount(-1);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        SceneManager.LoadScene("tutorial");
        if (FirebaseManager.instance != null)
        {
            FirebaseManager.instance.UpdateRetryCount(-1);
        }
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void NextLevel()
    {
        PlayerStats.levelNumber = 0;
        PlayerStats.IncreaseRetryCount(0);
        PlayerStats.ResetDeathCount(0);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        SceneManager.LoadScene("allyTutorial");
        if (FirebaseManager.instance != null)
        {
            FirebaseManager.instance.LogLevelStart(0);
        }
    }
}