using UnityEngine;
using UnityEngine.UI;

public class PreviewCamera : MonoBehaviour
{
    public RawImage previewRawImage;
    public float margin = 0.01f;

    private PlayerController player;
    private Camera mainCamera;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        Vector2 aimingLineEndPoint = player.endingPosition;
        transform.position = new Vector3(aimingLineEndPoint.x, aimingLineEndPoint.y, transform.position.z);

        Vector3 viewport = mainCamera.WorldToViewportPoint(aimingLineEndPoint);
        bool isOutside = viewport.x < 0f - margin || viewport.x > 1f + margin ||
            viewport.y < 0f - margin || viewport.y > 1f + margin;
        previewRawImage.enabled = isOutside && player.isReflected;
    }
}
