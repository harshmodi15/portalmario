using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HintButton : MonoBehaviour
{
    private Button hintButton;
    private Image buttonImage;
    // Start is called before the first frame update
    void Start()
    {
        hintButton = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        hintButton.onClick.AddListener(OnHintButtonClicked);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            hintButton.onClick.Invoke();
            if (buttonImage != null)
            {
                // grey out the button image
                buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
        }
    }
    private void OnHintButtonClicked()
    {
    }
    private void OnDestroy()
    {
        // Remove the listener when the object is destroyed to prevent memory leaks
        if (hintButton != null)
        {
            hintButton.onClick.RemoveListener(OnHintButtonClicked);
        }
    }
}
