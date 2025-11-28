using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsRespawn : MonoBehaviour
{
    [SerializeField] private float fallThreshold = -10f;
    private Vector2 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (transform.position.y < fallThreshold)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        transform.position = startPosition;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
}
