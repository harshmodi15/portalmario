using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public GameObject player;
    private EnemyController currentEnemy;

    public bool controllingPlayer = true;
    private PlayerController playerController;
    
    public PhysicsMaterial2D activeMaterial;
    public PhysicsMaterial2D inactiveMaterial;
    private Collider2D playerCollider;
    public GameObject popupText;
    private bool hasShownPopupText = false;

    void Start()
    {
        // Get playercontroller script attached to player
        playerController = player.GetComponent<PlayerController>();
        playerCollider = player.GetComponent<Collider2D>();
        // Probably won't need: playerCollider.sharedMaterial = activeMaterial;

        playerController.enabled = true;
        // playerRenderer.material = activeMaterial;
        
        // FOR OLD VERSION OF POPUP
        // if (popupText != null)
        // {
        //     popupText.SetActive(false);
        // }
    }

    void Update()
    {
        // Toggle control if left shift is pressed
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            ToggleControl();
            
            // FOR OLD VERSION OF POPUP
            // if(SceneManager.GetActiveScene().name == "allyTutorial" && popupText != null)
            // {
            //     popupText.SetActive(false);
            // }
        }
    }

    public void SetCurrentEnemy(EnemyController newEnemy)
    {
        // Changes control of enemy
        if (currentEnemy != null)
        {
            currentEnemy.OnDeathOrDisable -= OnEnemyLost;
            currentEnemy.enabled = false;
        }

        currentEnemy = newEnemy;

        if (currentEnemy != null)
        {
            currentEnemy.OnDeathOrDisable += OnEnemyLost;
            currentEnemy.enabled = false;
            
            Cage cage = FindObjectOfType<Cage>();

            // For explaining ally mechanic in ally tutorial
            if(SceneManager.GetActiveScene().name == "allyTutorial" && !hasShownPopupText && popupText != null && cage.enemyReleaseCount == 1)
            {
                // FOR OLD VERSION OF POPUP
                // popupText.SetActive(true);
                HintPopupManager.Instance.ShowHint(player.transform, "Left Shift = switch control between player and ally");
                hasShownPopupText = true;
            }

            // If we were already controlling an enemy, switch back to player first
            if (!controllingPlayer)
            {
                ToggleControl(); // back to player
            }
        }
    }

    private void OnEnemyLost()
    {
        if (!controllingPlayer) // Only act if we were controlling the enemy
        {
            ToggleControl(); // Switch back to player
        }
    }

    private void ToggleControl()
    {
        if (controllingPlayer)
        {
            if (currentEnemy != null && currentEnemy.gameObject.activeInHierarchy)
            {
                playerController.enabled = false;
                playerCollider.sharedMaterial = inactiveMaterial;
                playerController.lineRenderer.enabled = false;
                currentEnemy.enabled = true;
                controllingPlayer = false;
            }
        }
        else
        {
            if (playerController != null)
            {
                if (currentEnemy != null)
                    currentEnemy.enabled = false;

                playerController.enabled = true;
                playerCollider.sharedMaterial = activeMaterial;
                playerController.lineRenderer.enabled = true;
                controllingPlayer = true;
            }
        }
        //Debug.Log($"Shift Toggle. controllingPlayer: {controllingPlayer}. currentEnemy: {currentEnemy}");
    }
}