using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public sealed class CameraAreaMap2D : MonoBehaviour
{
    [Header("Area Defaults")]
    [Tooltip("1エリアぶんの幅と高さです。カメラ1画面に収まるサイズにします。")]
    [SerializeField] private Vector2 areaSize = new Vector2(17.77778f, 10f);
    [Tooltip("Grid PositionからArea Idを作るときの先頭文字です。")]
    [SerializeField] private string areaIdPrefix = "A";

    [Header("Create Area")]
    [Tooltip("Create At Gridで新規作成するエリアのグリッド座標です。")]
    [SerializeField] private Vector2Int newAreaGridPosition = new Vector2Int(1, 0);
    [Tooltip("Create At Gridで新規作成するエリアの表示名です。空ならArea Idを使います。")]
    [SerializeField] private string newAreaDisplayName = "";

    [Header("Managed Areas")]
    [Tooltip("このマップが管理するCameraArea2D一覧です。Collectで子オブジェクトから自動収集できます。")]
    [SerializeField] private CameraArea2D[] areas = Array.Empty<CameraArea2D>();

    public CameraArea2D[] Areas => areas;
    public Vector2 AreaSize => areaSize;
    public Vector2Int NewAreaGridPosition => newAreaGridPosition;

    public CameraArea2D FindAreaContaining(Vector2 worldPosition)
    {
        if (areas == null)
        {
            return null;
        }

        for (int i = 0; i < areas.Length; i++)
        {
            CameraArea2D area = areas[i];
            if (area != null && area.Contains(worldPosition))
            {
                return area;
            }
        }

        return null;
    }

    public void CollectAreasFromChildren()
    {
        areas = GetComponentsInChildren<CameraArea2D>(true);
        Array.Sort(areas, CompareAreas);
    }

    public string BuildAreaId(Vector2Int gridPosition)
    {
        return $"{areaIdPrefix}{gridPosition.x:+00;-00;00}_{gridPosition.y:+00;-00;00}";
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return transform.position + new Vector3(gridPosition.x * areaSize.x, gridPosition.y * areaSize.y, 0f);
    }

    private static int CompareAreas(CameraArea2D first, CameraArea2D second)
    {
        if (first == null && second == null)
        {
            return 0;
        }

        if (first == null)
        {
            return 1;
        }

        if (second == null)
        {
            return -1;
        }

        Vector2Int firstGrid = first.GridPosition;
        Vector2Int secondGrid = second.GridPosition;
        int yCompare = secondGrid.y.CompareTo(firstGrid.y);
        return yCompare != 0 ? yCompare : firstGrid.x.CompareTo(secondGrid.x);
    }

#if UNITY_EDITOR
    [ContextMenu("Collect Areas From Children")]
    public void CollectAreasFromChildrenMenu()
    {
        Undo.RecordObject(this, "Collect Camera Areas");
        CollectAreasFromChildren();
        EditorUtility.SetDirty(this);
    }

    [ContextMenu("Snap Children To Grid")]
    public void SnapChildrenToGrid()
    {
        CollectAreasFromChildren();

        for (int i = 0; i < areas.Length; i++)
        {
            CameraArea2D area = areas[i];
            if (area == null)
            {
                continue;
            }

            Undo.RecordObject(area.transform, "Snap Camera Area To Grid");
            area.transform.position = GridToWorldPosition(area.GridPosition);
            EditorUtility.SetDirty(area.transform);
        }
    }

    [ContextMenu("Rename Children From Grid")]
    public void RenameChildrenFromGrid()
    {
        CollectAreasFromChildren();

        for (int i = 0; i < areas.Length; i++)
        {
            CameraArea2D area = areas[i];
            if (area == null)
            {
                continue;
            }

            string areaId = BuildAreaId(area.GridPosition);
            Undo.RecordObject(area, "Rename Camera Area");
            Undo.RecordObject(area.gameObject, "Rename Camera Area");
            area.Configure(areaId, area.DisplayName, area.GridPosition, areaSize);
            area.gameObject.name = $"Camera Area {areaId}";
            EditorUtility.SetDirty(area);
            EditorUtility.SetDirty(area.gameObject);
        }
    }

    [ContextMenu("Create Area To Right")]
    public void CreateAreaToRight()
    {
        CollectAreasFromChildren();

        int nextX = 0;
        for (int i = 0; i < areas.Length; i++)
        {
            if (areas[i] != null)
            {
                nextX = Mathf.Max(nextX, areas[i].GridPosition.x + 1);
            }
        }

        CreateArea(new Vector2Int(nextX, 0));
    }

    [ContextMenu("Create Area At Grid")]
    public void CreateAreaAtGrid()
    {
        CreateArea(newAreaGridPosition);
    }

    private CameraArea2D CreateArea(Vector2Int gridPosition)
    {
        string areaId = BuildAreaId(gridPosition);
        string displayName = string.IsNullOrWhiteSpace(newAreaDisplayName) ? areaId : newAreaDisplayName;
        GameObject areaObject = new GameObject($"Camera Area {areaId}");
        Undo.RegisterCreatedObjectUndo(areaObject, "Create Camera Area");
        areaObject.transform.SetParent(transform);
        areaObject.transform.position = GridToWorldPosition(gridPosition);

        CameraArea2D area = areaObject.AddComponent<CameraArea2D>();
        area.Configure(areaId, displayName, gridPosition, areaSize);
        CollectAreasFromChildren();
        EditorUtility.SetDirty(this);
        return area;
    }
#endif

    private void Reset()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.orthographic)
        {
            float height = mainCamera.orthographicSize * 2f;
            areaSize = new Vector2(height * mainCamera.aspect, height);
        }

        CollectAreasFromChildren();
    }

    private void OnValidate()
    {
        if (areaSize.x < 0.01f)
        {
            areaSize.x = 0.01f;
        }

        if (areaSize.y < 0.01f)
        {
            areaSize.y = 0.01f;
        }
    }
}
