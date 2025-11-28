using Unity.Mathematics;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private Color bluePortalColor = Color.blue;
    [SerializeField] private Color orangePortalColor = new Color(1f, 0.5f, 0f, 1f);
    [SerializeField] private float teleportCooldown = 0.5f;

    private SpriteRenderer spriteRenderer;
    private PortalType type;
    private Portal linkedPortal;
    private float lastTeleportTime;
    private Vector2 portalNormal;

    private Vector2 previousFromPortal, previousToPortal;
    private int previousVelocity = 0;
    private int duplicationCount = 0, limitCount = 3;

    public PortalType Type => type;
    public Portal LinkedPortal
    {
        get => linkedPortal;
        set => linkedPortal = value;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(PortalType portalType, Vector2 normal)
    {
        type = portalType;
        portalNormal = normal;
        UpdatePortalColor();
    }

    private void UpdatePortalColor()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = type == PortalType.Blue ? bluePortalColor : orangePortalColor;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (linkedPortal == null) return;
        if (Time.time - lastTeleportTime < teleportCooldown) return;

        // Check if the object can be teleported
        if (other.CompareTag("Player") || other.CompareTag("Box")
            || (other.CompareTag("Hostility") && other.GetComponent<Renderer>().bounds.size.y <= 2f)
            )
        {
            TeleportObject(other);
        }
    }

    private void TeleportObject(Collider2D other)
    {
        // Get the object's rigidbody
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // Calculate teleport position
        Vector2 enterVelocity = rb.velocity;
        Vector2 teleportPosition = linkedPortal.transform.position;

        // exit with the same magnitude as entering, but in the direction of the linked portal's normal
        Vector2 exitVelocity = linkedPortal.portalNormal * enterVelocity.magnitude;
        // Teleport the object
        other.transform.position = teleportPosition + linkedPortal.portalNormal * math.max(transform.localScale.x, transform.localScale.y) * 1.5f;
        rb.velocity = exitVelocity;
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.fromPortal = true;
        }

        if (FirebaseManager.instance != null)
        {
            string objectType = "Unknown";
            if (other.CompareTag("Player")) objectType = "Player";
            else if (other.CompareTag("Box")) objectType = "Box";
            else if (other.CompareTag("Hostility")) objectType = "Enemy";

            Vector2 from = transform.position;
            Vector2 to = linkedPortal.transform.position;
            int level = PlayerStats.levelNumber;
            int velocity = (int)rb.velocity.magnitude;

            if (from.Equals(previousFromPortal) && to.Equals(previousToPortal) && previousVelocity == velocity)
            {
                duplicationCount++;
            } else {
                duplicationCount = 0;
            }

            if (duplicationCount < limitCount)
            {
                FirebaseManager.instance.LogPortalTraversal(objectType, from, to, velocity, level);
            }
            
            previousFromPortal = from;
            previousToPortal = to;
            previousVelocity = velocity;
        }
        // Update teleport cooldown
        lastTeleportTime = Time.time;
        linkedPortal.lastTeleportTime = Time.time;
    }
}
