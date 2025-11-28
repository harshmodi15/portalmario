using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UIElements;

public class MainCamera : MonoBehaviour
{
    public Transform player;
    [SerializeField] public float cameraMinX;
    [SerializeField] public float cameraMaxX;
    [SerializeField] public float cameraMinY;
    [SerializeField] public float cameraMaxY;
    [SerializeField] public float playerOffsetX;
    [SerializeField] public float playerOffsetY;
    public float smoothSpeed = 5f;
    public float zoomFactor = 1.5f;

    private PlayerManager playerManager;
    private PortalManager portalManager;
    private Camera mainCamera;
    private RectTransform timeCanvasRT;
    private float defaultOrthographicSize;

    private void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        portalManager = FindObjectOfType<PortalManager>();
        mainCamera = Camera.main;
        defaultOrthographicSize = mainCamera.orthographicSize;
        timeCanvasRT = transform.Find("TimeCanvas").GetComponent<RectTransform>();
    }

    void FixedUpdate()
    {
        GameObject allyObject = portalManager.GetCageCapturedObject();
        if (allyObject == null || allyObject.activeSelf == false)
        {
            UpdateViewFor(player);
            return;
        }

        float distance = Vector3.Distance(player.position, allyObject.transform.position);
        if (distance < 20f)
        {
            UpdatePlayerAndAllyView(allyObject.transform);
            return;
        }

        if (playerManager.controllingPlayer == true)
        {
            UpdateViewFor(player);
        }
        else
        {
            UpdateViewFor(allyObject.transform);
        }
    }

    private void UpdatePlayerAndAllyView(Transform ally)
    {
        // Compute the required zoom based on the distance
        Vector3 midpoint = (player.position + ally.position) / 2f;
        float distance = Vector3.Distance(player.position, ally.position);
        float newZoom = Mathf.Max(defaultOrthographicSize, distance / zoomFactor);

        // Smoothly interpolate camera position and zoom
        float positionY = transform.position.y;
        if (player.position.y - transform.position.y > playerOffsetY && transform.position.y < cameraMaxY)
        {
            positionY = player.position.y - playerOffsetY;
        }
        else if (transform.position.y - player.position.y > playerOffsetY && transform.position.y > cameraMinY)
        {
            positionY = player.position.y + playerOffsetY;
        }

        transform.position = Vector3.Lerp(transform.position, new Vector3(midpoint.x, positionY, transform.position.z), Time.deltaTime * smoothSpeed);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, newZoom, Time.deltaTime * smoothSpeed);

        // Adjust TimeCanvas scale to match camera zoom
        float scaleFactor = mainCamera.orthographicSize / defaultOrthographicSize;
        timeCanvasRT.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
    }

    private void UpdateViewFor(Transform target)
    {
        timeCanvasRT.localScale = new Vector3(1f, 1f, 1f);
        float positionX = transform.position.x;
        float positionY = transform.position.y;

        if (target.position.x - transform.position.x > playerOffsetX && transform.position.x < cameraMaxX)
        {
            positionX = target.position.x - playerOffsetX;
        }
        else if (transform.position.x - target.position.x > playerOffsetX && transform.position.x > cameraMinX)
        {
            positionX = target.position.x + playerOffsetX;
        }

        if (target.position.y - transform.position.y > playerOffsetY && transform.position.y < cameraMaxY)
        {
            positionY = target.position.y - playerOffsetY;
        }
        else if (transform.position.y - target.position.y > playerOffsetY && transform.position.y > cameraMinY)
        {
            positionY = target.position.y + playerOffsetY;
        }

        // Adjust PositionY high enough
        float floorThreshold = target.position.y >= 7f ? 10f : 0f;
        positionY = Mathf.Max(positionY, floorThreshold);
        positionX = Mathf.Max(positionX, cameraMinX);
        transform.position = Vector3.Lerp(transform.position, new Vector3(positionX, positionY, transform.position.z), Time.deltaTime * smoothSpeed);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, defaultOrthographicSize, Time.deltaTime * smoothSpeed);
    }

    public void ResetPlayer(Transform respawnedPlayer)
    {
        player = respawnedPlayer;
    }
}
