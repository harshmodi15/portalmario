using System;
using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class HintPopupManager : MonoBehaviour
{
    public static HintPopupManager Instance;
    private Canvas popupCanvas;
    public GameObject hintPrefab;
    public GameObject hintButtonPrefab;
    public GameObject hintButton;
    public Vector3 hintOffset = new Vector3(0, 3f, 0);
    public bool hasPressedX = false;

    private GameObject currentHint;
    private Coroutine hintCoroutine;
    private Dictionary<string,int> hintShowCounts = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Find popup canvas and assign
        GameObject canvasObject = GameObject.Find("PopupCanvas");
        if(canvasObject != null)
        {
            popupCanvas = canvasObject.GetComponent<Canvas>();
        }
        else
        {
            Debug.LogWarning("Popup Canvas not found in scene");
        }
    }

    public void ShowHintButton(Transform player, Action onDismiss = null)
    {
        if (popupCanvas == null || hintButtonPrefab == null)
        {
            Debug.LogWarning("Popup Canvas or hintPrefab not assigned");
            return;
        }

        if (currentHint != null)
        {
            Destroy(currentHint);
            currentHint = null;
        }
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
        }

        currentHint = Instantiate(hintButtonPrefab, popupCanvas.transform);
        hintButton = currentHint;
        currentHint.SetActive(true);

        RectTransform hintRect = currentHint.GetComponent<RectTransform>();

        hintRect.anchorMin = hintRect.anchorMax = new Vector2(0.5f, 0f);
        hintRect.pivot = new Vector2(1, 0);

        float offsetX = 0f;
        float offsetY = 25f;
        hintRect.anchoredPosition = new Vector2(-offsetX, offsetY);

    }

    public void HideHintButton()
    {
        if (hintButton != null)
        {
            Destroy(hintButton);
        }
    }

    public void ShowHint(Transform player, string hintText, Action onDismiss = null, int maxTimesToShow = int.MaxValue)
    {
        
        if (popupCanvas == null || hintPrefab == null)
        {
            Debug.LogWarning("Popup Canvas or hintPrefab not assigned");
            return;
        }

        // Create a dictionary entry for text if it doesn't exist
        if(!hintShowCounts.ContainsKey(hintText))
        {
            hintShowCounts[hintText] = 0;
        }

        // If it's over the max show limit, return and don't show hint
        if(hintShowCounts[hintText] >= maxTimesToShow)
        {
            return;
        }

        hintShowCounts[hintText]++;

        // Check to see if there is an active hint already and destroy
        if(currentHint != null)
        {
            Destroy(currentHint);
            currentHint = null;
        }
        
        // Same for coroutine
        if(hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            currentHint = null;
        }

        // Instantiate UI Hint
        popupCanvas.transform.SetParent(player, false);
        currentHint = Instantiate(hintPrefab, popupCanvas.transform);

        // Set text dynamically
        TextMeshProUGUI hintTextComponent = currentHint.GetComponentInChildren<TextMeshProUGUI>();
        if (hintTextComponent != null)
        {
            hintTextComponent.text = hintText;
        }
        else
        {
            Debug.LogWarning("No TextMeshProUGUI found on hint prefab!");
        }

        // Convert world position to screen position for placement above player
        Vector3 worldPos = player.position + hintOffset;
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            popupCanvas.GetComponent<RectTransform>(),
            Camera.main.WorldToScreenPoint(worldPos),
            Camera.main,
            out anchoredPos
        );

        // Apply to RectTransform
        RectTransform hintRect = currentHint.GetComponent<RectTransform>();
        hintRect.anchoredPosition = anchoredPos;
        currentHint.SetActive(true); // enable hint since prefab is not active by default

        hintCoroutine = StartCoroutine(HintFollowsPlayer(player, onDismiss));
    }

    public IEnumerator HintButtonFollowsPlayer(Transform player, Action onDismiss)
    {
        RectTransform hintRect = currentHint.GetComponent<RectTransform>();
        RectTransform canvasRect = popupCanvas.GetComponent<RectTransform>();

        while (currentHint != null)
        {
            // World position above player
            Vector3 worldPos = player.position + hintOffset;

            // Convert to canvas local point
            Vector2 anchoredPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                Camera.main.WorldToScreenPoint(worldPos),
                Camera.main,
                out anchoredPos
            );

            hintRect.anchoredPosition = anchoredPos;


            yield return null;
        }
    }
    public IEnumerator HintFollowsPlayer(Transform player, Action onDismiss)
    {
        RectTransform hintRect = currentHint.GetComponent<RectTransform>();
        RectTransform canvasRect = popupCanvas.GetComponent<RectTransform>();

        while (currentHint != null)
        {
            // World position above player
            Vector3 worldPos = player.position + hintOffset;

            // Convert to canvas local point
            Vector2 anchoredPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                Camera.main.WorldToScreenPoint(worldPos),
                Camera.main,
                out anchoredPos
            );

            hintRect.anchoredPosition = anchoredPos;

            // Wait until space is pressed
            if (Input.GetKeyDown(KeyCode.X))
            {
                Destroy(currentHint);
                currentHint = null;
                StopCoroutine(hintCoroutine);
                onDismiss?.Invoke(); // calls the input function if necessary
                yield break;
            }

            yield return null;
        }
    }
}