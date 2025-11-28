using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] public float speed;
    [SerializeField] public float minX;
    [SerializeField] public float maxX;
    [SerializeField] public float rotationSpeed;

    private Vector3 direction = Vector3.right;

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        if (transform.position.x >= maxX)
        {
            direction = Vector3.left;
        }
        else if (transform.position.x <= minX)
        {
            direction = Vector3.right;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Portal") || collision.gameObject.CompareTag("Cage")
            || (collision.gameObject.CompareTag("Mirror") && collision.transform.parent == null))
        {
            collision.transform.parent = transform;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Box") || collision.gameObject.CompareTag("Mirror") || collision.gameObject.CompareTag("Hostility") || collision.gameObject.CompareTag("RedEnemy"))
        {
            collision.transform.position += direction * speed * Time.deltaTime;
        }
    }
}