using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private float fallThreshold = -10f;
    private Vector2 respawnPosition;
    private Vector2[] checkpoints;
    private GameObject[] checkpointAreas;
    private int nextCheckpointIndex = 0;
    private int frameCounter = 0;
    private int CheckpointUpdateInterval = 5;

    private Coroutine blinkCoroutine;
    private float blinkDuration = 1.5f;
    private float blinkInterval = 0.1f;

    void Start()
    {
        respawnPosition = transform.position;
        if (SceneManager.GetActiveScene().name == "tutorial")
        {
            checkpoints = new Vector2[] { new Vector2(14f, -1.5f), new Vector2(45.5f, -1.5f), new Vector2(72f, -1.5f) };
        }
        else if (SceneManager.GetActiveScene().name == "allyTutorial")
        {
            checkpoints = new Vector2[] { new Vector2(33f, -1.5f), new Vector2(60f, -1.5f) };
        }
        else if (SceneManager.GetActiveScene().name == "lvl1")
        {
            checkpoints = new Vector2[] { new Vector2(18f, 2.3f), new Vector2(43f, -1.5f) };
        }
        else if (SceneManager.GetActiveScene().name == "lvl2")
        {
            checkpoints = new Vector2[] { new Vector2(8f, -1.5f), new Vector2(40f, -2.5f), new Vector2(42f, 8.7f), new Vector2(11f, 8.7f) };
        }

        GameObject checkpointPrefab = Resources.Load<GameObject>("Sprites/CheckpointArea");
        checkpointAreas = new GameObject[checkpoints.Length];
        for (int i = 0; i < checkpoints.Length; i++)
        {
            checkpointAreas[i] = Instantiate(checkpointPrefab, checkpoints[i], Quaternion.identity);
        }
    }

    void Update()
    {
        UpdateCheckpoint();
        if (transform.position.y < fallThreshold)
        {
            LogDeath("Fall");
            Respawn();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log("Collision detected with: " + other.gameObject.name);
        if (other.CompareTag("Trap") || other.CompareTag("Laser")) 
        {
            LogDeath("Trap");
            Respawn();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Debug.Log("Collision detected with: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Hostility") && collision.gameObject.layer != LayerMask.NameToLayer("Companion") )
        {
            if (collision.gameObject.GetComponent<HeadTrigger>() == null)
            {
                LogDeath("Enemy");
                Respawn();
            }
        }
    }

    void LogDeath(string reason)
    {
        if (FirebaseManager.instance != null)
        {
            Vector2 pos = transform.position;
            float time = Time.timeSinceLevelLoad;
            int level = PlayerStats.levelNumber;
            DeathReasonData deathData = new DeathReasonData(reason, pos, time);
            FirebaseManager.instance.LogTestDataByPOST("deathReasons", deathData, level);
        }
    }

    void UpdateCheckpoint()
    {
        frameCounter++;
        if (frameCounter >= CheckpointUpdateInterval || nextCheckpointIndex >= checkpoints.Length)
        {
            return;
        }
        frameCounter = 0;

        Vector2 nextCheckpoint = checkpoints[nextCheckpointIndex];
        int playerFloor = transform.position.y >= 7f ? 1 : 0;
        int nextCheckpointFloor = nextCheckpoint.y >= 7f ? 1 : 0;
        bool reachCheckpoint = false;
        if (nextCheckpointFloor == 0)
        {
            if (transform.position.x - nextCheckpoint.x > 0 && playerFloor == nextCheckpointFloor)
            {
                reachCheckpoint = true;
            }
        }
        else
        {
            if (transform.position.x - nextCheckpoint.x < 0 && playerFloor == nextCheckpointFloor)
            {
                reachCheckpoint = true;
            }
        }

        if (reachCheckpoint)
        {
            respawnPosition = nextCheckpoint;
            Destroy(checkpointAreas[nextCheckpointIndex]);
            nextCheckpointIndex++;
        }
    }

    IEnumerator Blink()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        float elapsed = 0f;
        while (elapsed < blinkDuration)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = true; // Ensure visible at end
    }

    public void Respawn()
    {
        if (FirebaseManager.instance != null)
        {
            PlayerStats.IncreaseDeathCount(PlayerStats.levelNumber);
            FirebaseManager.instance.UpdateDeathCount(PlayerStats.levelNumber);
        }

        transform.position = respawnPosition;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        BreakableGlassPlatform[] platforms = FindObjectsOfType<BreakableGlassPlatform>(true);
        foreach (BreakableGlassPlatform platform in platforms)
        {
            platform.ResetPlatform();
        }

        // Remvoe portals upon respawn for anticheating
        PortalManager portalManager = FindObjectOfType<PortalManager>();
        if (portalManager != null)
        {
            portalManager.RemovePortals();
        }


        // EnemyController enemyController = FindObjectOfType<EnemyController>(true);
        // if (enemyController != null)
        // {
        //     // Enemy enemy = enemyController.GetComponent<Enemy>();
        //     // enemy.gameObject.SetActive(true);
        //     // enemy.TakeDamage(9999f);

        //     // return to cage
        //     GameObject captured = portalManager.GetCageCapturedObject();
        //     if (captured != null)
        //     {
        //         portalManager.SetCageCapturedObject(captured);
        //         Destroy(captured);
        //         portalManager.GetCageCapturedObject().SetActive(false);
        //         portalManager.GetActiveCage().SetIsCaptured(true);
        //     }
        // }
        // Instead of destroying the ally, just hide it and reset the cage

        
        // Cage cage = portalManager.GetActiveCage();
        // if (cage != null)
        // {
        //     cage.gameObject.SetActive(false);
        // }

        Cage cage = portalManager.GetActiveCage();
        if (cage != null && cage.capturedObject != null)
        {
            // Detach from world, re-parent under the cage
            cage.capturedObject.transform.SetParent(cage.transform);
            cage.capturedObject.transform.localPosition = Vector3.zero;

            // Deactivate it so it's “in the cage” again
            cage.capturedObject.SetActive(false);

            // Mark the cage as holding something
            cage.SetIsCaptured(true);
        }

        // Finally, hide the cage itself
        if (cage != null)
        {
            cage.gameObject.SetActive(false);
        }


        MainCamera cameraScript = Camera.main.GetComponent<MainCamera>();
        if (cameraScript != null)
        {
            cameraScript.ResetPlayer(transform);
        }

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        blinkCoroutine = StartCoroutine(Blink());
    }
}