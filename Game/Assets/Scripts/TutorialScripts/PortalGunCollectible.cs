using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PortalGunCollectible : MonoBehaviour
{
    public GameObject player;
    public string popupText;
    //public PopupManager popupManager;
    private HintPopupManager hintPopupManager;

    private PortalManager portalManager;

    void Start()
    {
        portalManager = FindObjectOfType<PortalManager>();
        hintPopupManager = FindObjectOfType<HintPopupManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            LineRenderer line = other.GetComponent<LineRenderer>();
            if (SceneManager.GetActiveScene().name == "tutorial")
            {
                portalManager.CanUsePortal = true;
                if (line != null)
                {
                    line.enabled = true;
                }
            }
            else if (SceneManager.GetActiveScene().name == "allyTutorial")
            {
                portalManager.CanUseCage = true;
            }
            else if (SceneManager.GetActiveScene().name == "lvl2")
            {
                portalManager.CanUseMirror = true;
            }

            // if (popupManager != null)
            // {
            //     popupManager.ShowPopupText();
            // }
            if (hintPopupManager != null)
            {
                HintPopupManager.Instance.ShowHint(other.transform, popupText);
            }

            Destroy(gameObject);
        }
    }


}
