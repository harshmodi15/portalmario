using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class HintZone : MonoBehaviour
{
    public GameObject hint;
    public float timeToShowHint = 5f;
    public Vector3 hintOffset = new Vector3(0, 3f, 0);
    public string hintText = "Enter hint text in inspector";
    public int maxTimesToShow = int.MaxValue;

    private float timeInside = 0f;
    private bool hintShown = false;
    private bool hasExceededMaxTimes = false;
    private bool hasPressedX = false;
    private bool hasShownHintButton = false;
    private bool hasCancelled = false;
    private GameObject currentHint;
    private PlayerController playerController;
    private Canvas popupCanvas;
    private Coroutine hintCoroutine;
    private Transform playerTransform;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.X) && hasExceededMaxTimes)
        {
            hasPressedX = true;
            HintPopupManager.Instance.HideHintButton();
        }
        
    }
    // private void Start()
    // {
    //     GameObject canvasObject = GameObject.Find("PopupCanvas");
    //     if(canvasObject != null)
    //     {
    //         popupCanvas = canvasObject.GetComponent<Canvas>();
    //     }
    //     else
    //     {
    //         Debug.LogWarning("Popup Canvas not found in scene");
    //     }
    // }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = other.transform;
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hintShown)
        {
            timeInside += Time.deltaTime;

            if (timeInside >= timeToShowHint)
            {
                hasExceededMaxTimes = true;
                if (!hasCancelled && hasPressedX)
                {
                    hintShown = true;
                    playerController = other.GetComponent<PlayerController>();
                    //ShowHint(other.transform);
                    HintPopupManager.Instance.ShowHint(other.transform, hintText, ResetHint, maxTimesToShow);   
                }
                else if (!hasCancelled && !hasPressedX && !hasShownHintButton)
                {
                    // Show hint button
                    HintPopupManager.Instance.ShowHintButton(other.transform, ResetHint);
                    hasShownHintButton = true;
                }
           }

        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ResetHint();
        }
    }

    // private void ShowHint(Transform player)
    // {
        
    //     if (popupCanvas == null)
    //     {
    //         Debug.LogWarning("Popup Canvas not assigned");
    //         return;
    //     }

    //     // Check to see if there is an active hint already and destroy
    //     if(currentHint != null)
    //     {
    //         Destroy(currentHint);
    //     }
        
    //     // Same for coroutine
    //     if(hintCoroutine != null)
    //     {
    //         StopCoroutine(hintCoroutine);
    //     }

    //     // Instantiate UI Hint
    //     currentHint = Instantiate(hint, popupCanvas.transform);
    //     currentHint.SetActive(true); // enable hint since prefab is not active by default

    //     // Set text dynamically
    //     TextMeshProUGUI hintTextComponent = currentHint.GetComponentInChildren<TextMeshProUGUI>();
    //     if (hintTextComponent != null)
    //     {
    //         hintTextComponent.text = hintText;
    //     }
    //     else
    //     {
    //         Debug.LogWarning("No TextMeshProUGUI found on hint prefab!");
    //     }

    //     // Convert world position to screen position for placement above player
    //     Vector3 worldPos = player.position + hintOffset;
    //     Vector2 anchoredPos;
    //     RectTransformUtility.ScreenPointToLocalPointInRectangle(
    //         popupCanvas.GetComponent<RectTransform>(),
    //         Camera.main.WorldToScreenPoint(worldPos),
    //         Camera.main,
    //         out anchoredPos
    //     );

    //     // Apply to RectTransform
    //     RectTransform hintRect = currentHint.GetComponent<RectTransform>();
    //     hintRect.anchoredPosition = anchoredPos;

    //     hintCoroutine = StartCoroutine(HintFollowsPlayer(player));
    // }

    // private IEnumerator HintFollowsPlayer(Transform player)
    // {
    //     RectTransform hintRect = currentHint.GetComponent<RectTransform>();
    //     RectTransform canvasRect = popupCanvas.GetComponent<RectTransform>();

    //     while (currentHint != null)
    //     {
    //         // World position above player
    //         Vector3 worldPos = player.position + hintOffset;

    //         // Convert to canvas local point
    //         Vector2 anchoredPos;
    //         RectTransformUtility.ScreenPointToLocalPointInRectangle(
    //             canvasRect,
    //             Camera.main.WorldToScreenPoint(worldPos),
    //             Camera.main,
    //             out anchoredPos
    //         );

    //         hintRect.anchoredPosition = anchoredPos;

    //         // Wait until space is pressed
    //         if (Input.GetKeyDown(KeyCode.Space))
    //         {
    //             Destroy(currentHint);
    //             ResetHint();
    //             yield break;
    //         }

    //         yield return null;
    //     }
    // }
    

    private void ResetHint()
    {        
        //hasShownHintButton = false;
        timeInside = 0f;
        hintShown = false;
        hasCancelled = false;
        hasPressedX = false;
        hasExceededMaxTimes = false;
        HintPopupManager.Instance.HideHintButton();
    }
}
