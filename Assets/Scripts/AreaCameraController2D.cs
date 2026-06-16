using UnityEngine;

[DisallowMultipleComponent]
public sealed class AreaCameraController2D : MonoBehaviour
{
    [Header("Inspector References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform player;
    [SerializeField] private CameraAreaMap2D areaMap;
    [SerializeField] private CameraArea2D[] areas;

    [Header("Transition")]
    [SerializeField] private float transitionDuration = 0f;

    [Header("Startup")]
    [SerializeField] private bool autoFindReferences = true;
    [SerializeField] private bool autoDiscoverAreas = true;

    private CameraArea2D currentArea;
    private Vector3 velocity;

    public CameraArea2D CurrentArea => currentArea;
    public Bounds CurrentAreaBounds => currentArea != null ? currentArea.Bounds : GetCameraBounds();

    private void Awake()
    {
        ResolveReferences();
        currentArea = FindAreaContainingPlayer();
        MoveCameraToCurrentArea(true);
    }

    private void LateUpdate()
    {
        if (targetCamera == null || player == null)
        {
            ResolveReferences();
            if (targetCamera == null || player == null)
            {
                return;
            }
        }

        CameraArea2D nextArea = FindAreaContainingPlayer();
        if (nextArea != null && nextArea != currentArea)
        {
            currentArea = nextArea;
            velocity = Vector3.zero;
        }

        MoveCameraToCurrentArea(false);
    }

    public CameraArea2D FindAreaContainingPlayer()
    {
        if (player == null)
        {
            return null;
        }

        Vector2 playerPosition = player.position;

        if (areaMap != null)
        {
            CameraArea2D mapArea = areaMap.FindAreaContaining(playerPosition);
            if (mapArea != null)
            {
                return mapArea;
            }
        }

        if (areas == null)
        {
            return currentArea;
        }

        for (int i = 0; i < areas.Length; i++)
        {
            CameraArea2D area = areas[i];
            if (area != null && area.Contains(playerPosition))
            {
                return area;
            }
        }

        return currentArea;
    }

    private void MoveCameraToCurrentArea(bool immediate)
    {
        if (targetCamera == null || currentArea == null)
        {
            return;
        }

        Vector3 targetPosition = currentArea.CameraPosition;
        targetPosition.z = targetCamera.transform.position.z;

        if (immediate || transitionDuration <= 0f)
        {
            targetCamera.transform.position = targetPosition;
            return;
        }

        targetCamera.transform.position = Vector3.SmoothDamp(
            targetCamera.transform.position,
            targetPosition,
            ref velocity,
            transitionDuration);
    }

    private Bounds GetCameraBounds()
    {
        if (targetCamera == null)
        {
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        float halfHeight = targetCamera.orthographicSize;
        float halfWidth = halfHeight * targetCamera.aspect;
        Vector3 cameraPosition = targetCamera.transform.position;
        Vector3 center = new Vector3(cameraPosition.x, cameraPosition.y, 0f);
        Vector3 size = new Vector3(halfWidth * 2f, halfHeight * 2f, 1f);
        return new Bounds(center, size);
    }

    private void ResolveReferences()
    {
        if (!autoFindReferences)
        {
            return;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (areaMap == null)
        {
            areaMap = FindFirstObjectByType<CameraAreaMap2D>();
        }

        if (player == null)
        {
            PlayerMovement2D playerMovement = FindFirstObjectByType<PlayerMovement2D>();
            if (playerMovement != null)
            {
                player = playerMovement.transform;
            }
        }

        if (autoDiscoverAreas && (areas == null || areas.Length == 0))
        {
            if (areaMap != null)
            {
                areaMap.CollectAreasFromChildren();
                areas = areaMap.Areas;
            }
            else
            {
                areas = FindObjectsByType<CameraArea2D>(FindObjectsSortMode.InstanceID);
            }
        }
    }

    private void Reset()
    {
        targetCamera = GetComponent<Camera>();
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        PlayerMovement2D playerMovement = FindFirstObjectByType<PlayerMovement2D>();
        if (playerMovement != null)
        {
            player = playerMovement.transform;
        }

        areaMap = FindFirstObjectByType<CameraAreaMap2D>();
        areas = areaMap != null
            ? areaMap.Areas
            : FindObjectsByType<CameraArea2D>(FindObjectsSortMode.InstanceID);
    }
}
