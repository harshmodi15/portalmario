using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GetComponentInParent<EnemyController>() != null) return;
        if (collision.CompareTag("Player"))
        {
            float damage = collision.GetComponent<PlayerController>().GetCurrentVelocityMagnitude();
            transform.parent.GetComponent<Enemy>().TakeDamage(damage, collision.gameObject);
            Debug.Log("Player hit the enemy's head and did NOT die.");

            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = new Vector2(rb.velocity.x, 6f);
            }
        }
        // Kill RedEnemy instantly if hit by Box
        else if (collision.CompareTag("Box")) 
        {
            Enemy enemy = transform.parent.GetComponent<Enemy>();
            Rigidbody2D boxRb = collision.GetComponent<Rigidbody2D>();
            // Instantly kill RedEnemy

            if (boxRb != null)
            {
                float boxSpeed = boxRb.velocity.magnitude;
                float requiredSpeed = 7f;
                float normalspeed = 1f;

                Debug.Log("Box speed: " + boxSpeed);

                // Only kill RedEnemy if the Box is moving fast enough
                if (enemy is RedEnemy) 
                {
                    if (boxSpeed >= requiredSpeed) 
                    {
                        Debug.Log("RedEnemy hit on head by high-speed box! Instantly dying.");
                        enemy.TakeDamage(9999f, collision.gameObject);
                    }
                    else if (boxSpeed >= normalspeed) {
                        Debug.Log("Box hit detected! counting as one hit");
                        enemy.TakeDamage(1, collision.gameObject);
                    }
                    else
                    {
                        Debug.Log("Box hit RedEnemy but was too slow!");
                    }
                }
                else
                {
                    enemy.TakeDamage(1f, collision.gameObject);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
