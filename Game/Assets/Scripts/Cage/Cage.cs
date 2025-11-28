using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Cage : MonoBehaviour
{
    private bool isCaptured;
    private float lastReleaseTime;
    private float releaseCooldown = 0.5f;
    private Color enemyColor = new Color(0.58f, 0.16f, 0.9f);
    private Color companionColor = new Color(0.9f, 0.4f, 0.15f);
    private PlayerManager playerManager;
    public Vector2 normal { get; set; }
    public GameObject capturedObject;
    // Below is for allytutorial
    public int enemyReleaseCount = 0;
    private bool hasCapturedFirstEnemy = false;

    private AllyUIManager allyUI;
    private EnemyController currentAlly;
   
    // Start is called before the first frame update
    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        isCaptured = false;
        capturedObject = null;
        // allyUI = FindObjectOfType<AllyUIManager>();
        allyUI = FindObjectOfType<AllyUIManager>();
        currentAlly = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (capturedObject == null) isCaptured = false;
        if (isCaptured || Time.time - lastReleaseTime < releaseCooldown) return;
        if (capturedObject != null && other.gameObject != capturedObject && IsCapturedObject(other.gameObject))
        {
            if (capturedObject.CompareTag("Hostility"))
            {
                Destroy(capturedObject.GetComponent<EnemyController>());
                capturedObject.layer = LayerMask.NameToLayer("Default");
                capturedObject.GetComponent<SpriteRenderer>().color = enemyColor;

                // REMOVE happy face
                Transform happyFace = capturedObject.transform.Find("HappyFace(Clone)");
                if (happyFace != null)
                {
                    Destroy(happyFace.gameObject);
                }

                // ADD back angry face
                GameObject angryFacePrefab = Resources.Load<GameObject>("Sprites/AngryFace");
                if (angryFacePrefab != null)
                {
                    GameObject angryFace = Instantiate(angryFacePrefab, Vector3.zero, Quaternion.identity);
                    angryFace.transform.SetParent(capturedObject.transform);
                    angryFace.transform.localPosition = Vector3.zero;
                    angryFace.transform.localRotation = Quaternion.identity;
                }

                // Make ally turned enemy get enemy movements again
                Enemy enemyAIControls = capturedObject.GetComponent<Enemy>();
                if(enemyAIControls != null)
                {
                    enemyAIControls.enabled = true;
                }
            }
        }
        if (IsCapturedObject(other.gameObject))
        {
            // Destroy the box and clone it to the cage
            if (currentAlly != null)
            {
                currentAlly.OnDeathOrDisable -= HandleAllyDeath;
                currentAlly = null;
            }
            
            capturedObject = Instantiate(other.gameObject);
            AllyAnimation allyAnimation = other.gameObject.GetComponent<AllyAnimation>();
            if (allyAnimation == null)
            {
                other.gameObject.AddComponent<AllyAnimation>();
            }
            other.gameObject.GetComponent<AllyAnimation>().StartShrink();
            if (capturedObject.CompareTag("Hostility") && capturedObject.layer != LayerMask.NameToLayer("Companion"))
            {
                if (FirebaseManager.instance != null)
                {
                    Vector2 pos = transform.position;
                    int level = PlayerStats.levelNumber;
                    FirebaseManager.instance.LogEnemyKill("Converted to Ally", pos, level, "PurpleEnemy");
                }

                // Check to see if it is allytutorial and show popup if so
                Debug.Log("Scene name: " + SceneManager.GetActiveScene().name);
                if(SceneManager.GetActiveScene().name == "allyTutorial" && !hasCapturedFirstEnemy)
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    // Maybe include safety to check if popupmanager exists
                    //HintPopupManager.Instance.ShowHint(player.transform, "Great! You've captured your first enemy.");
                    hasCapturedFirstEnemy = true;
                    //Debug.Log("Attempted to show release hint");
                }
                // EnemyController newController = capturedObject.AddComponent<EnemyController>();

                EnemyController newController = capturedObject.AddComponent<EnemyController>();
                capturedObject.AddComponent<AllyAnimation>();


                // — subscribe our handler so we get notified when *this* ally dies —
                // if (currentAlly != null)
                //     currentAlly.OnDeathOrDisable -= HandleAllyDeath;
                currentAlly = newController;
                currentAlly.OnDeathOrDisable += HandleAllyDeath;


                playerManager.SetCurrentEnemy(newController);
                capturedObject.layer = LayerMask.NameToLayer("Companion");

                if (allyUI != null)
                {
                    allyUI.AllyCaptured();
                }

                capturedObject.GetComponent<SpriteRenderer>().color = companionColor;

                Transform angryFace = capturedObject.transform.Find("AngryFace");
                Quaternion faceRotation = Quaternion.identity;
                if (angryFace != null)
                {
                    faceRotation = angryFace.transform.rotation;
                    Destroy(angryFace.gameObject);
                }

                GameObject happyFacePrefab = Resources.Load<GameObject>("Sprites/HappyFace");
                GameObject happyFace = Instantiate(happyFacePrefab, Vector3.zero, Quaternion.identity);
                happyFace.transform.SetParent(capturedObject.transform);
                happyFace.transform.localPosition = Vector3.zero;
                happyFace.transform.localRotation = faceRotation;
            }
            capturedObject.SetActive(false);
            isCaptured = true;
        }
    }

    private bool IsCapturedObject(GameObject obj)
    {
        //return obj.CompareTag("Box") || (obj.CompareTag("Hostility") && !Enemy.IsTallEnemy(obj));
        return obj.CompareTag("Hostility") && !Enemy.IsTallEnemy(obj);
    }
    public void Release()
    {
        if (!isCaptured || capturedObject == null) 
        {
            return;
        }
        lastReleaseTime = Time.time;
        capturedObject.transform.position = transform.position + new Vector3(normal.x, normal.y, 0) * 1f;
        capturedObject.SetActive(true);
        capturedObject.GetComponent<AllyAnimation>().StartGrow();
        isCaptured = false;
        enemyReleaseCount++;
        playerManager.SetCurrentEnemy(capturedObject.GetComponent<EnemyController>());
        // Remove enemy AI controls (patrol and speed)
        // Enemy enemyAIControls = capturedObject.GetComponent<Enemy>();
        // if(enemyAIControls != null)
        // {
        //     enemyAIControls.enabled = false;
        // }
    }
    
    public void SetIsCaptured(bool value)
    {
        isCaptured = value;
    }

    private void HandleAllyDeath()
    {
        allyUI?.ReleaseAlly();
        // unsubscribe so we don’t fire again
        if (currentAlly != null)
           currentAlly.OnDeathOrDisable -= HandleAllyDeath;
        currentAlly = null;
    }
}
