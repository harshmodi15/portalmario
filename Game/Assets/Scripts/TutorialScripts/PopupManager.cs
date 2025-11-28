using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PopupManager : MonoBehaviour
{
    public GameObject popupText;

    public void ShowPopupText()
    {
        popupText.SetActive(true);
    }

    public void HidePopupText()
    {
        popupText.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(popupText.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            popupText.SetActive(false);
        }
    }
}
