using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float maxShootingDistance = 50f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask portalLayer;
    [SerializeField] private LayerMask trapLayer;

    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private LayerMask boxLayer;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float interactionRange = 2f;

    public static PlayerController instance;
    private Rigidbody2D rb;
    private bool isGrounded;
    private float horizontalInput;
    private bool jumpPressed;
    private ThrowableBox heldBox;
    private SpriteRenderer spriteRenderer;
    private float currentVelocityMagnitude;
    private bool isLineVisible;
    public Vector2 endingPosition { get; set; }
    public Vector2 intermediatePosition { get; set; }
    public bool fromPortal;
    public LineRenderer lineRenderer;
    public Vector2 intersectionPoint { get; set; }
    public bool isReflected = false;

    // public int maxReflections = 5;
    [SerializeField] private LayerMask mirrorLayer;
    private PlayerManager playerManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        fromPortal = false;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerManager = FindObjectOfType<PlayerManager>();
        if (!groundCheck)
        {
            groundCheck = transform;
        }
        if (lineRenderer == null)
        {
            isLineVisible = true;
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            Color beigeColor = new Color(0.9f, 0.8f, 0.7f, 1f); 
            lineRenderer.startColor = beigeColor;
            lineRenderer.endColor = beigeColor;
            lineRenderer.sortingOrder = 100;
        }
    }

    private void Start()
    {
        // Disable line of sight in tutorial before shooting is introduced
        if(SceneManager.GetActiveScene().name == "tutorial")
        {
            GetComponent<LineRenderer>().enabled = false;
        }
    }

    private void Update()
    {
        if (playerManager.controllingPlayer)
        {
            // Get movement input
            // horizontalInput = Input.GetAxisRaw("Horizontal");
            horizontalInput = 0f;
            if (Input.GetKey(KeyCode.A))
                horizontalInput = -1f;
            else if (Input.GetKey(KeyCode.D))
                horizontalInput = 1f;

            // if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W))
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
            {
                jumpPressed = true;
                if (FirebaseManager.instance != null)
                {
                    Vector2 pos = transform.position;
                    float time = Time.timeSinceLevelLoad;
                    int level = PlayerStats.levelNumber;

                    JumpEventData jumpData = new JumpEventData(pos, time);
                    FirebaseManager.instance.LogTestDataByPOST("jumps", jumpData, level);
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                Vector2 pos = transform.position;
                float time = Time.timeSinceLevelLoad;
                int level = PlayerStats.levelNumber;

                if (FirebaseManager.instance != null)
                {
                    MirrorUseEvent mirrorData = new MirrorUseEvent(pos, time);
                    FirebaseManager.instance.LogTestDataByPOST("mirror", mirrorData, level);
                }
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                Vector2 pos = transform.position;
                float time = Time.timeSinceLevelLoad;
                int level = PlayerStats.levelNumber;

                if (FirebaseManager.instance != null)
                {
                    AllyUseEvent Data = new AllyUseEvent(pos, time);
                    FirebaseManager.instance.LogTestDataByPOST("ally", Data, level);
                }
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                Vector2 pos = transform.position;
                float time = Time.timeSinceLevelLoad;
                int level = PlayerStats.levelNumber;

                if (FirebaseManager.instance != null)
                {
                    LOSUseEvent LOSData = new LOSUseEvent(pos, time);
                    FirebaseManager.instance.LogTestDataByPOST("toggle_LOS", LOSData, level);
                }
            }

            // Ground check
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            // Handle box interaction
            // if (Input.GetKeyDown(interactKey))
            // {
            //     if (heldBox == null)
            //     {
            //         TryPickupBox();
            //     }
            //     else
            //     {
            //         DropBox();
            //     }
            // }

            // Flip sprite based on movement direction
            if (horizontalInput != 0)
            {
                spriteRenderer.flipX = horizontalInput < 0;
            }
            
            currentVelocityMagnitude = rb.velocity.magnitude;

            // Draw line
            if (Input.GetKeyDown(KeyCode.F))
            {
                isLineVisible = !isLineVisible;
                lineRenderer.enabled = isLineVisible;
            }
        }

        DrawLineOfSight();
    }

    // private void OnCollisionEnter2D(Collision2D collision)
    // {
    //     if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
    //     {
    //         // Calculate damage based on velocity
    //         float damage = currentVelocityMagnitude;
    //         // Try to apply damage to enemy
    //         Enemy enemy = collision.gameObject.GetComponent<Enemy>();
    //         if (enemy != null)
    //         {
    //             enemy.TakeDamage(damage);
    //         }
    //     }
    // }        // Check if we hit an enemy

    private void FixedUpdate()
    {
        // Handle movement
        Vector2 moveVelocity = rb.velocity;
        if (fromPortal)
        {
            //moveVelocity.x = moveVelocity.x + horizontalInput * moveSpeed * 0.05f;
            // To make the movement smoother, abandon the previous velocity
            moveVelocity.x = horizontalInput * moveSpeed;
        }
        else
        {
            moveVelocity.x = horizontalInput * moveSpeed;
        }
        if (isGrounded && fromPortal)
        {
            fromPortal = false;
        }
        // Handle jumping
        if (jumpPressed && isGrounded)
        {
            moveVelocity.y = jumpForce;
            jumpPressed = false;
        }
        else if (!isGrounded)
        {
            // Prevent jumping in mid-air
            jumpPressed = false;
        }

        rb.velocity = moveVelocity;
    }

    private void DrawLineOfSight()
    {
        // Due to the added mirror functionality, we need to get the ending position of the line even if the line is not visible
        // if (isLineVisible)
        {
            Vector2 start = transform.position;
            Vector2 direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
            List<Vector3> linePositions = new List<Vector3> { start };
            float remainingDistance = maxShootingDistance;
            isReflected = false;

            while (remainingDistance > 0)
            {
                RaycastHit2D hit = Physics2D.Raycast(start, direction, remainingDistance, portalLayer | mirrorLayer | trapLayer);
                
                if (hit.collider == null)
                {
                    linePositions.Add(start + direction * remainingDistance);
                    break;
                }

                linePositions.Add(hit.point);

                // If we hit a mirror, reflect and continue
                if (((1 << hit.collider.gameObject.layer) & mirrorLayer) != 0)
                {
                    isReflected = true;
                    direction = Vector2.Reflect(direction, hit.normal);
                    start = hit.point + direction * 0.01f; // Offset to prevent self-hits
                    remainingDistance -= hit.distance;
                }
                else
                {
                    break; // Hit something other than a mirror, stop tracing
                }
            }
            if (AimLineIntersectsWithLaser())
            {
                linePositions[^1] = intersectionPoint;
            }
            endingPosition = linePositions[^1];
            intermediatePosition = linePositions[^2];
            lineRenderer.positionCount = linePositions.Count;
            lineRenderer.SetPositions(linePositions.ToArray());
        }
    }

    private void TryPickupBox()
    {
        // Check for nearby boxes
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRange, boxLayer);
        
        foreach (Collider2D collider in colliders)
        {
            ThrowableBox box = collider.GetComponent<ThrowableBox>();
            if (box != null && box.TryPickup(transform))
            {
                heldBox = box;
                break;
            }
        }
    }

    private void DropBox()
    {
        if (heldBox != null)
        {
            // The box's ThrowBox method will be called if player clicks
            heldBox = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize ground check
        if (groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Visualize interaction range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    public float GetCurrentVelocityMagnitude()
    {
        return currentVelocityMagnitude;
    }
    
    // Method to check if player's aiming line intersects with any laser in the scene
    public bool AimLineIntersectsWithLaser()
    {
        // Gather all laser controllers in the scene
        LaserController[] lasers = FindObjectsOfType<LaserController>();
        if (lasers.Length == 0)
        {
            return false;
        }
            
        
        // Reconstruct the aiming line using the same logic as DrawLineOfSight
        Vector2 start = transform.position;
        Vector2 direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        List<Vector2> lineSegmentStarts = new List<Vector2>();
        List<Vector2> lineSegmentEnds = new List<Vector2>();
        float remainingDistance = maxShootingDistance;
        
        // Build line segments the same way we draw them
        Vector2 currentStart = start;
        while (remainingDistance > 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentStart, direction, remainingDistance, portalLayer | mirrorLayer);
            
            Vector2 currentEnd;
            if (hit.collider == null)
            {
                currentEnd = currentStart + direction * remainingDistance;
                lineSegmentStarts.Add(currentStart);
                lineSegmentEnds.Add(currentEnd);
                break;
            }
            
            currentEnd = hit.point;
            lineSegmentStarts.Add(currentStart);
            lineSegmentEnds.Add(currentEnd);
            
            // If we hit a mirror, reflect and continue
            if (((1 << hit.collider.gameObject.layer) & mirrorLayer) != 0)
            {
                direction = Vector2.Reflect(direction, hit.normal);
                currentStart = hit.point + direction * 0.01f; // Offset to prevent self-hits
                remainingDistance -= hit.distance;
            }
            else
            {
                break; // Hit something other than a mirror, stop tracing
            }
        }
        
        
        // Check each segment of the player's aim line against each laser
        for (int i = 0; i < lineSegmentStarts.Count; i++)
        {
            Vector2 segmentStart = lineSegmentStarts[i];
            Vector2 segmentEnd = lineSegmentEnds[i];
            
            foreach (LaserController laser in lasers)
            {
                if (laser.isOn && laser.IntersectsWithLine(segmentStart, segmentEnd))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
}