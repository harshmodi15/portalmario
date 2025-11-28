using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PortalManager : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private GameObject mirrorPrefab;
    [SerializeField] private GameObject cagePrefab;
    [SerializeField] private LayerMask portalPlacementMask;

    [SerializeField] private LayerMask portalClickMask;  

    [SerializeField] private LayerMask mirrorPlacementMask;
    [SerializeField] private float minPortalDistance = 1f;

    private List<Portal> activePortals = new List<Portal>();
    private GameObject activeMirror;
    private GameObject activeCage;
    private Camera mainCamera;
    private PlayerController player;

    public bool CanUsePortal { get; set; } = true;
    public bool CanUseCage { get; set; } = true;
    public bool CanUseMirror { get; set; } = true;

    private void Start()
    {
        mainCamera = Camera.main;
        player = FindObjectOfType<PlayerController>();
        activeCage = null;

        if (SceneManager.GetActiveScene().name == "tutorial")
        {
            CanUsePortal = false;
            CanUseCage = false;
            CanUseMirror = false;
        }
        else if (SceneManager.GetActiveScene().name == "allyTutorial")
        {
            CanUseCage = false;
            CanUseMirror = false;
        }
        else if (SceneManager.GetActiveScene().name == "lvl1" || SceneManager.GetActiveScene().name == "lvl2")
        {
            CanUseMirror = false;
        }
    }

    private void Update()
    {
        HandleGunCreation();
    }

    private void HandleGunCreation()
    {
        if (player == null || !FindObjectOfType<PlayerManager>().controllingPlayer)
        {
            return;
        }

        // Visual indicator in game view
        if (player.AimLineIntersectsWithLaser())
        {
            return;
        }
        // Left click for blue portal
        if (Input.GetMouseButtonDown(0) && CanUsePortal)
        {
            CreatePortal(PortalType.Blue);
        }
        // Right click for orange portal
        else if (Input.GetMouseButtonDown(1) && CanUsePortal)
        {
            CreatePortal(PortalType.Orange);
        }
        // E click for mirror
        else if (Input.GetKeyDown(KeyCode.E) && CanUseMirror)
        {
            CreateMirror();
        }
        // C click for cage
        else if (Input.GetKeyDown(KeyCode.C) && CanUseCage)
        {
            CreateCage();
            if (activeCage.GetComponent<Cage>().capturedObject != null)
            {
                activeCage.GetComponent<Cage>().Release();
            }
        }
        // // R click to remove mirror
        // else if (Input.GetKeyDown(KeyCode.R))
        // {
        //     if (activeCage != null)
        //     {
        //         activeCage.GetComponent<Cage>().Release();
        //     }
        // }
    }

    private RaycastHit2D GetGunRaycastHit(LayerMask layerMask)
    {
        Vector2 startPosition = player.intermediatePosition;
        Vector2 endPosition = player.endingPosition;
        Vector2 direction = (endPosition - startPosition).normalized;
        return Physics2D.Raycast(startPosition, direction, Mathf.Infinity, layerMask);
    }

    private void CreateCage()
    {
        RaycastHit2D hit = GetGunRaycastHit(portalPlacementMask);
        if (hit.collider != null)
        {
            if (!IsValidCagePosition(hit.point))
                return;
            if (hit.transform.CompareTag("NoPortalSurface"))
                return;

            if (hit.transform.CompareTag("Trap") || hit.transform.gameObject.layer == LayerMask.NameToLayer("Trap"))
                return;

            // Create new cage
            Vector2 normal = hit.normal;
            float cageRotation = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg + 90f;
            if (activeCage == null)
            {
                GameObject cageObj = Instantiate(cagePrefab, hit.point, Quaternion.Euler(0, 0, cageRotation));
                cageObj.AddComponent<Cage>();
                activeCage = cageObj;
            }
            else
            {
                activeCage.transform.parent = null; // Workaround for re-positioning from a moving platform

                // Move existing cage
                activeCage.transform.position = hit.point;
                activeCage.transform.rotation = Quaternion.Euler(0, 0, cageRotation);
            }
            activeCage.GetComponent<Cage>().normal = normal;
            activeCage.SetActive(true);
            
        }
    }

    private bool IsValidCagePosition(Vector2 position)
    {

        // Check if the cage is within main camera's view
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(position);
        if (viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1 || viewportPoint.z < 0)
        {
            return false;
        }

        return true;
    }
    private void CreateMirror()
    {
        RaycastHit2D hit = GetGunRaycastHit(mirrorPlacementMask);
        if (hit.collider != null)
        {
            if (player.isReflected)
            {
                //Debug.Log("Tried to make a mirror through reflection");
                return;
            }

            if (hit.transform.CompareTag("NoPortalSurface"))
                return;

            if (hit.transform.CompareTag("Trap") || hit.transform.gameObject.layer == LayerMask.NameToLayer("Trap"))
                return;

            // Check if we can place a mirror here
            if (!IsValidMirrorPosition(hit.point))
                return;

            // Remove existing mirror if it exists
            RemoveMirror();

            // Create new mirror
            Vector2 normal = hit.normal;
            float mirrorRotation = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg + 90f;
            GameObject mirrorObj = Instantiate(mirrorPrefab, hit.point, Quaternion.Euler(0, 0, mirrorRotation));
            if (hit.transform.CompareTag("Box"))
            {
                mirrorObj.transform.parent = hit.transform;
            }
            activeMirror = mirrorObj;
        }
    }

    private bool IsValidMirrorPosition(Vector2 position)
    {
        // Check distance from other mirrors
        if (activeMirror != null)
        {
            if (Vector2.Distance(position, activeMirror.transform.position) < minPortalDistance)
            {
                return false;
            }
        }

        // Check if the mirror is within main camera's view
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(position);
        if (viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1 || viewportPoint.z < 0)
        {
            return false;
        }

        return true;
    }

    // private void RemoveMirror()
    public void RemoveMirror()
    {
        if (activeMirror != null)
        {
            Destroy(activeMirror);
            activeMirror = null;
        }
    }

    // private void CreatePortal(PortalType type)
    // {
    //     if (Time.timeScale == 0f) return;

    //     RaycastHit2D hit = GetGunRaycastHit(portalPlacementMask);
    //     if (hit.collider != null)
    //     {
    //         if (hit.transform.CompareTag("NoPortalSurface"))
    //             return;

    //         if (hit.transform.CompareTag("Trap") || hit.transform.gameObject.layer == LayerMask.NameToLayer("Trap"))
    //             return;

    //         RaycastHit2D belowHit = Physics2D.Raycast(hit.point + Vector2.down * 0.1f, Vector2.down, 0.2f, portalPlacementMask);
    //         if (belowHit.collider != null && (belowHit.collider.CompareTag("Trap") || belowHit.collider.gameObject.layer == LayerMask.NameToLayer("Trap")))
    //             return;

    //         // Check if we can place a portal here
    //         if (!IsValidPortalPosition(hit.point))
    //             return;

    //         // Remove existing portal of same type if it exists
    //         RemovePortalOfType(type);

    //         Vector2 normal = hit.normal;
    //         float portalRotation = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg + 90f;
    //         // Create new portal
    //         GameObject portalObj = Instantiate(portalPrefab, hit.point, Quaternion.Euler(0, 0, portalRotation));
    //         Portal portal = portalObj.GetComponent<Portal>();
    //         portal.Initialize(type, normal);
    //         activePortals.Add(portal);

    //         // Link portals if we have a pair
    //         if (activePortals.Count == 2)
    //         {
    //             LinkPortals();
    //         }
    //     }
    // }

    private void CreatePortal(PortalType type)
    {
        if (Time.timeScale == 0f) return;

        // --- A) Mouse → World on the correct Z‐plane ---
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = -mainCamera.transform.position.z;
        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(screenPos);

        // --- B) Exact collider click test ---
        Collider2D click = Physics2D.OverlapPoint(mouseWorld, portalClickMask);
        if (click != null)
        {
            Portal p = click.GetComponent<Portal>();
            if (p != null && p.Type == type)
            {
                RemovePortalOfType(type);
                return;
            }
        }

        // --- C) Fallback: small‐radius distance check in case collider is tiny ---
        const float clickRadius = 0.3f;
        foreach (Portal p in activePortals)
        {
            if (p.Type == type &&
                Vector2.Distance(mouseWorld, p.transform.position) <= clickRadius)
            {
                RemovePortalOfType(type);
                return;
            }
        }

        // --- D) (unchanged) your normal surface‐placement & instantiation logic below ---
        RaycastHit2D hit = GetGunRaycastHit(portalPlacementMask);
        if (hit.collider == null) return;
        if (hit.transform.CompareTag("NoPortalSurface")) return;
        if (hit.transform.CompareTag("Trap") ||
            hit.transform.gameObject.layer == LayerMask.NameToLayer("Trap")) return;

        RaycastHit2D belowHit = Physics2D.Raycast(
            hit.point + Vector2.down * 0.1f,
            Vector2.down, 0.2f,
            portalPlacementMask
        );
        if (belowHit.collider != null &&
            (belowHit.collider.CompareTag("Trap") ||
            belowHit.collider.gameObject.layer == LayerMask.NameToLayer("Trap")))
            return;

        if (!IsValidPortalPosition(hit.point)) return;

        RemovePortalOfType(type);

        Vector2 normal = hit.normal;
        float portalRotation = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg + 90f;
        GameObject obj = Instantiate(
            portalPrefab,
            hit.point,
            Quaternion.Euler(0, 0, portalRotation)
        );
        Portal portal = obj.GetComponent<Portal>();
        portal.Initialize(type, normal);
        activePortals.Add(portal);

        if (activePortals.Count == 2)
            LinkPortals();
    }






    private bool IsValidPortalPosition(Vector2 position)
    {
        // Check distance from other portals
        foreach (Portal portal in activePortals)
        {
            if (Vector2.Distance(position, portal.transform.position) < minPortalDistance)
            {
                return false;
            }
        }

        // Check if the portal is within main camera's view
        // or is reflected by the mirro and showed in previewCamera
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(position);
        if (!player.isReflected && (viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1 || viewportPoint.z < 0))
        {
            return false;
        }

        return true;
    }

    private void RemovePortalOfType(PortalType type)
    {
        Portal portalToRemove = activePortals.Find(p => p.Type == type);
        if (portalToRemove != null)
        {
            activePortals.Remove(portalToRemove);
            Destroy(portalToRemove.gameObject);
        }
    }

    private void LinkPortals()
    {
        if (activePortals.Count != 2) return;

        Portal portal1 = activePortals[0];
        Portal portal2 = activePortals[1];

        portal1.LinkedPortal = portal2;
        portal2.LinkedPortal = portal1;
    }

    // For removing portals particularly upon respawn
    public void RemovePortals()
    {
        foreach(Portal portal in activePortals)
        {
            Destroy(portal.gameObject);
        }
        activePortals.Clear();
    }

    public Cage GetActiveCage()
    {
        if (activeCage == null)
        {
            return null;
        }

        Cage cage = activeCage.GetComponent<Cage>();
        if (cage == null)
        {
            return null;
        }

        return cage;
    }

    public GameObject GetCageCapturedObject()
    {
        if (activeCage == null)
        {
            return null;
        }

        Cage cage = activeCage.GetComponent<Cage>();
        if (cage == null)
        {
            return null;
        }

        return cage.capturedObject;
    }

    public void SetCageCapturedObject(GameObject capturedObject)
    {
        if (activeCage == null)
        {
            return;
        }

        Cage cage = activeCage.GetComponent<Cage>();
        if (cage == null)
        {
            return;
        }

        cage.capturedObject = Instantiate(capturedObject);
    }
}

public enum PortalType
{
    Blue,
    Orange
}