using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserButton : MonoBehaviour
{
    public LaserController laser;
    // private bool objectOnButton = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //objectOnButton = true;
        laser.SetActive(false);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //objectOnButton = false;
        laser.SetActive(true);
    }
}
