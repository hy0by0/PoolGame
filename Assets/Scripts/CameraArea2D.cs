using UnityEngine;

[DisallowMultipleComponent]
public sealed class CameraArea2D : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string areaId = "A00";
    [SerializeField] private string displayName = "Area";
    [SerializeField] private Vector2Int gridPosition;

    [Header("Area")]
    [SerializeField] private Vector2 size = new Vector2(17.77778f, 10f);
    [SerializeField] private Vector2 offset;

    [Header("Camera")]
    [SerializeField] private Transform cameraAnchor;

    public string AreaId => areaId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? areaId : displayName;
    public Vector2Int GridPosition => gridPosition;
    public Vector2 Size => size;

    public Bounds Bounds
    {
        get
        {
            Vector3 center = transform.position + (Vector3)offset;
            return new Bounds(center, new Vector3(size.x, size.y, 1f));
        }
    }

    public Vector3 CameraPosition
    {
        get
        {
            Vector3 anchorPosition = cameraAnchor != null ? cameraAnchor.position : Bounds.center;
            return new Vector3(anchorPosition.x, anchorPosition.y, 0f);
        }
    }

    public void Configure(string nextAreaId, string nextDisplayName, Vector2Int nextGridPosition, Vector2 nextSize)
    {
        areaId = nextAreaId;
        displayName = nextDisplayName;
        gridPosition = nextGridPosition;
        size = nextSize;
    }

    public bool Contains(Vector2 worldPosition)
    {
        Bounds bounds = Bounds;
        return worldPosition.x >= bounds.min.x
            && worldPosition.x <= bounds.max.x
            && worldPosition.y >= bounds.min.y
            && worldPosition.y <= bounds.max.y;
    }

    private void Reset()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.orthographic)
        {
            float height = mainCamera.orthographicSize * 2f;
            size = new Vector2(height * mainCamera.aspect, height);
        }
    }

    private void OnDrawGizmos()
    {
        Bounds bounds = Bounds;
        Gizmos.color = new Color(0.15f, 0.75f, 1f, 0.22f);
        Gizmos.DrawCube(bounds.center, bounds.size);
        Gizmos.color = new Color(0.15f, 0.75f, 1f, 0.9f);
        Gizmos.DrawWireCube(bounds.center, bounds.size);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            bounds.center + Vector3.up * (bounds.extents.y - 0.45f),
            $"{areaId}  {DisplayName}\nGrid {gridPosition.x}, {gridPosition.y}");
#endif
    }
}
