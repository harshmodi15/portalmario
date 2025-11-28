using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PauseButtonHandler : MonoBehaviour
{
    public GameObject pauseMenuCanvas;
    private PlayerController playerController;


    private bool isPaused = false;

    private void Start()
    {
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false); 
    }

    private void Update()
    {
        // Listen for Escape key press
        playerController = FindObjectOfType<PlayerController>();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        if (pauseMenuCanvas == null)
        {
            Debug.LogWarning("Pause Menu Canvas is not assigned!");
            return;
        }

        isPaused = !isPaused;
        pauseMenuCanvas.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        if (playerController != null && playerController.lineRenderer != null)
        {
            playerController.lineRenderer.enabled = !isPaused;
        }

        Debug.Log("Pause toggled: " + isPaused);
    }

    public void ResumeGame()
    {
        pauseMenuCanvas.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Debug.Log("Resume is clicked");

        if (playerController != null && playerController.lineRenderer != null)
        {
            playerController.lineRenderer.enabled = true;
        }
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f;
        PlayerStats.ResetDeathCount(PlayerStats.levelNumber);
        PlayerStats.IncreaseRetryCount(PlayerStats.levelNumber);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        if (FirebaseManager.instance != null)
        {
            FirebaseManager.instance.LogLevelStart(PlayerStats.levelNumber);
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); 
    }
}
