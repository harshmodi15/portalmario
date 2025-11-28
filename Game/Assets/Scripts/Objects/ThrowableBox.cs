using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class ThrowableBox : MonoBehaviour
{
    [Header("Box Settings")]
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private LayerMask enemyLayer;

    private Rigidbody2D rb;
    private bool isHeld;
    private Transform holder;
    private Vector3 originalScale;
    private float currentVelocityMagnitude;
    private bool isPushed;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
        isPushed = false;
    }

    private void Update()
    {
        if (isHeld)
        {
            // Update position to follow holder
            transform.position = holder.position + Vector3.up * 0.5f;

            // Check for throw input
            if (Input.GetMouseButtonDown(0))
            {
                ThrowBox();
            }
        }

        // Update current velocity magnitude for damage calculation
        currentVelocityMagnitude = rb.velocity.magnitude;
    }

    public bool TryPickup(Transform newHolder)
    {
        if (isHeld) return false;

        float distance = Vector2.Distance(transform.position, newHolder.position);
        if (distance <= pickupRange)
        {
            holder = newHolder;
            isHeld = true;
            rb.isKinematic = true;
            transform.localScale = originalScale * 0.8f; // Slight scale down when held
            return true;
        }
        return false;
    }

    private void ThrowBox()
    {
        isHeld = false;
        holder = null;
        rb.isKinematic = false;
        transform.localScale = originalScale;

        // Get mouse position in world space
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 throwDirection = (mousePos - (Vector2)transform.position).normalized;

        // Apply throw force
        rb.velocity = throwDirection * throwForce;
    }


    // private void OnCollisionEnter2D(Collision2D collision)
    // {
    //     if (collision.gameObject.layer == LayerMask.NameToLayer("Mirror"))
    //     {
    //         collision.transform.parent = transform;
    //     }
    // }
    private void OnCollisionStay2D(Collision2D collision)
    {
        // Check if we hit an enemy
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            // Calculate damage based on velocity
            float damage = currentVelocityMagnitude * damageMultiplier;
            if (isPushed)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            // Try to apply damage to enemy
            HeadTrigger headTrigger = collision.gameObject.GetComponent<HeadTrigger>();
            if (headTrigger != null)
            {
                // headTrigger.transform.parent.GetComponent<Enemy>().TakeDamage(damage);
                Enemy enemy = headTrigger.transform.parent.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, collision.gameObject);
                }
            }
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            isPushed = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            rb.constraints = RigidbodyConstraints2D.None;
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            isPushed = false;
            rb.constraints = RigidbodyConstraints2D.None;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize pickup range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}