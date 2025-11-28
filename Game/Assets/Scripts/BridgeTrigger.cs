using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeTrigger : MonoBehaviour
{
    public GameObject bridge; // Reference to the bridge GameObject
    // Start is called before the first frame update
    void Start()
    {
        bridge.SetActive(false); // Deactivate the bridge at the start
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        // Destroy the bridge GameObject when this object is destroyed
        if (bridge != null)
        {
            bridge.SetActive(true); // Deactivate the bridge
        }
    }
}
