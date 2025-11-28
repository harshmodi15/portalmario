using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player touched RedEnemy's body! Respawn");
            PlayerRespawn player = collision.gameObject.GetComponent<PlayerRespawn>();

            if (player != null)
            {
                player.Respawn(); 
            }
            else
            {
                Debug.LogError("PlayerRespawn script not found on Player!");
            }
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
