using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialCompleteTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            LevelTimer timer = FindObjectOfType<LevelTimer>();
            if (timer != null)
            {
                timer.StopTimer();
            }

            float completionTime = timer != null ? timer.GetTime() : 0f;

            if (FirebaseManager.instance != null)
            {
                FirebaseManager.instance.UpdateLevelCompletion(-1, completionTime);
            }

            SceneManager.LoadScene("TutorialComplete");
        }
    }
}
