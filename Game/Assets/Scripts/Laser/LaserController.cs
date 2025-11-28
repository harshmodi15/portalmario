using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    public Transform laserOrigin;
    public LineRenderer lineRenderer;
    public bool isOn = true;
    [SerializeField] private float maxShootingDistance = 25f;

    // Laser angle and oscillation
    private float startingAngle;
    public bool oscillate = false;
    public float oscillationSpeed = 0.5f;
    public float oscillationAngle = 45f;

    private PlayerRespawn playerRespawn;

    [SerializeField] private string playerTag = "Player";
    [SerializeField] private LayerMask mirrorLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask boxLayer;
    [SerializeField] private LayerMask trapLayer;
    [SerializeField] private LayerMask enemyLayer;

    
    // Store laser line segments for intersection checking
    private List<Vector3> laserPositions = new List<Vector3>();

    void Awake()
    {
        playerRespawn = FindObjectOfType<PlayerRespawn>();
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            //lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.sortingOrder = 100;

            // Set corner vertices to 2 to make the line renderer look smoother
            lineRenderer.numCornerVertices = 2;
        }
        if (laserOrigin == null)
        {
            laserOrigin = transform;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        startingAngle = laserOrigin.eulerAngles.z;
    }

    // Update is called once per frame
    void Update()
    {
        if(!isOn)
        {
            lineRenderer.enabled = false;
            return;
        }

        if (oscillate)
        {
            float angleOffset = Mathf.Sin(Time.time * oscillationSpeed) * oscillationAngle;
            laserOrigin.rotation = Quaternion.Euler(0, 0, startingAngle + angleOffset);
        }
        
        DrawLaser();
    }

    void DrawLaser()
    {
        lineRenderer.enabled = true;
        Vector2 start = laserOrigin.position;
        Vector2 direction = laserOrigin.up;
        
        // Clear the previous list
        laserPositions.Clear();
        laserPositions.Add(start);
        
        float remainingDistance = maxShootingDistance;

        while (remainingDistance > 0)
        {
            int mask = groundLayer | mirrorLayer | boxLayer | trapLayer | enemyLayer | (1 << LayerMask.NameToLayer("Default"));
            RaycastHit2D hit = Physics2D.Raycast(start, direction, remainingDistance, mask);
            
            if (hit)
            {
                laserPositions.Add(hit.point);
                if (((1 << hit.collider.gameObject.layer) & mirrorLayer) != 0)
                {
                    direction = Vector2.Reflect(direction, hit.normal);
                    start = hit.point + direction * 0.05f; // Offset to prevent self-hits
                    remainingDistance -= hit.distance;
                    continue;
                }

                if (hit.collider.CompareTag(playerTag))
                {
                    if (FirebaseManager.instance != null)
                    {
                        Vector2 pos = hit.point;
                        float time = Time.timeSinceLevelLoad;
                        int level = PlayerStats.levelNumber;
                        string reason = "Laser";

                        DeathReasonData deathData = new DeathReasonData(reason, pos, time);
                        FirebaseManager.instance.LogTestDataByPOST("deathReasons", deathData, level);
                    }

                    playerRespawn.Respawn();
                    break;
                }
                
                if (hit.collider.CompareTag("Hostility") || hit.collider.GetComponent<HeadTrigger>() != null)
                {
                    Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
                    if(enemy != null)
                    {
                        enemy.TakeDamage(100, this.gameObject);
                    }
                    break;
                }

                // just continue if we hit a portal
                if (hit.collider.CompareTag("Portal") || hit.collider.CompareTag("Cage") || hit.collider.CompareTag("HintZone"))
                {
                    start = hit.point + direction * 0.05f; // Offset to prevent self-hits
                    remainingDistance -= hit.distance;
                    continue;
                }

                break;
            }
            else
            {
                laserPositions.Add(start + direction * remainingDistance);
                break;
            }
        }
        
        // Set the positions for the line renderer
        lineRenderer.positionCount = laserPositions.Count;
        lineRenderer.SetPositions(laserPositions.ToArray());
    }

    // For button presses
    public void SetActive(bool state)
    {
        isOn = state;
    }
    
    // Check if a line segment intersects with any part of the laser beam
    public bool IntersectsWithLine(Vector2 lineStart, Vector2 lineEnd)
    {
        if (!isOn || laserPositions.Count < 2)
            return false;
            
        // Check each segment of the laser against the provided line segment
        for (int i = 0; i < laserPositions.Count - 1; i++)
        {
            Vector2 laserSegmentStart = laserPositions[i];
            Vector2 laserSegmentEnd = laserPositions[i + 1];
            
            if (LineSegmentsIntersect(lineStart, lineEnd, laserSegmentStart, laserSegmentEnd))
            {
                PlayerController.instance.intersectionPoint = GetIntersectionPoint(lineStart, lineEnd, laserSegmentStart, laserSegmentEnd);
                return true;
            }
        }
        
        return false;
    }
    
    // Helper method to check if two line segments intersect
    private bool LineSegmentsIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        // Line AB represented as a1 + t * (a2 - a1)
        // Line CD represented as b1 + s * (b2 - b1)
        
        float denominator = (b2.y - b1.y) * (a2.x - a1.x) - (b2.x - b1.x) * (a2.y - a1.y);
        
        // Lines are parallel or collinear
        if (Mathf.Approximately(denominator, 0f))
            return false;
            
        float ua = ((b2.x - b1.x) * (a1.y - b1.y) - (b2.y - b1.y) * (a1.x - b1.x)) / denominator;
        float ub = ((a2.x - a1.x) * (a1.y - b1.y) - (a2.y - a1.y) * (a1.x - b1.x)) / denominator;
        
        // Check if intersection point is on both line segments
        return ua >= 0f && ua <= 1f && ub >= 0f && ub <= 1f;
    }

    // Helper method to get the intersection point of two line segments
    public Vector2 GetIntersectionPoint(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        // Line AB represented as a1 + t * (a2 - a1)
        // Line CD represented as b1 + s * (b2 - b1)
        
        float denominator = (b2.y - b1.y) * (a2.x - a1.x) - (b2.x - b1.x) * (a2.y - a1.y);
        
        // Lines are parallel or collinear
        if (Mathf.Approximately(denominator, 0f))
            return Vector2.zero;
            
        float ua = ((b2.x - b1.x) * (a1.y - b1.y) - (b2.y - b1.y) * (a1.x - b1.x)) / denominator;
        float ub = ((a2.x - a1.x) * (a1.y - b1.y) - (a2.y - a1.y) * (a1.x - b1.x)) / denominator;
        
        // Check if intersection point is on both line segments
        if (ua >= 0f && ua <= 1f && ub >= 0f && ub <= 1f)
        {
            return new Vector2(a1.x + ua * (a2.x - a1.x), a1.y + ua * (a2.y - a1.y));
        }
        
        return Vector2.zero;
    }
}