using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableGlassPlatform : MonoBehaviour
{
    [SerializeField] private float blinkDuration = 2.5f; // Time before breaking
    [SerializeField] private float blinkInterval = 0.1f; // Blink speed

    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;
    private bool isBreaking = false;
    private bool isBroken = false;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private PortalManager portalManager;

    void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        portalManager = FindObjectOfType<PortalManager>();
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Box")) && !isBreaking && !isBroken)
        {
            StartCoroutine(BlinkAndBreak());
        }
    }

    IEnumerator BlinkAndBreak()
    {
        isBreaking = true;
        float elapsedTime = 0f;

        while (elapsedTime < blinkDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
        }

        spriteRenderer.enabled = false;
        platformCollider.enabled = false;
        isBroken = true;
        isBreaking = false;

        // yield return new WaitForSeconds(0.05f); // Let objects settle

        // Collider2D[] overlapping = Physics2D.OverlapBoxAll(transform.position, transform.localScale, 0f);
        // foreach (var col in overlapping)
        // {
        //     bool isTagged = col.CompareTag("Portal") || col.CompareTag("Mirror") || col.CompareTag("Cage");
        //     bool hasPortal = col.GetComponent<Portal>() != null;
        //     bool isMirrorLayer = col.gameObject.layer == LayerMask.NameToLayer("Mirror");
        //     bool isTrapLayer = col.gameObject.layer == LayerMask.NameToLayer("Trap");

        //     if (isTagged || hasPortal || isMirrorLayer || isTrapLayer)
        //     {
        //         Destroy(col.gameObject);
        //     }
        // }
        if (portalManager != null)
        {
            portalManager.RemovePortals();
            portalManager.RemoveMirror();

            var cage = portalManager.GetActiveCage();
            if (cage != null)
            {
                Destroy(cage.gameObject);
            }
        }
        
    }

    public void ResetPlatform()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        spriteRenderer.enabled = true;
        platformCollider.enabled = true;
        isBreaking = false;
        isBroken = false;
    }
}