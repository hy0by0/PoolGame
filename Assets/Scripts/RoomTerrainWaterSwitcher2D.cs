using UnityEngine;

// カメラが見ている1部屋内の地形と水をまとめて切り替えるクラス。
[DisallowMultipleComponent]
public sealed class RoomTerrainWaterSwitcher2D : MonoBehaviour
{
    [Header("Inspector References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private TerrainWaterSwitchable2D[] switchables;

    [Header("Camera Area")]
    [SerializeField] private float cameraAreaPadding = 0.05f;

    // 現在カメラ内にある切り替え対象だけを反転する。
    public void ToggleCurrentCameraArea()
    {
        Bounds cameraBounds = GetCameraBounds();

        for (int i = 0; i < switchables.Length; i++)
        {
            if (Intersects2D(cameraBounds, switchables[i].SwitchBounds))
            {
                switchables[i].ToggleState();
            }
        }
    }

    // Orthographic Camera の表示範囲をワールド座標のBoundsとして返す。
    private Bounds GetCameraBounds()
    {
        float halfHeight = targetCamera.orthographicSize + cameraAreaPadding;
        float halfWidth = halfHeight * targetCamera.aspect + cameraAreaPadding;
        Vector3 cameraPosition = targetCamera.transform.position;
        Vector3 center = new Vector3(cameraPosition.x, cameraPosition.y, 0f);
        Vector3 size = new Vector3(halfWidth * 2f, halfHeight * 2f, 1f);
        return new Bounds(center, size);
    }

    // Z位置に関係なく、X/Yの範囲が重なっているかを調べる。
    private static bool Intersects2D(Bounds first, Bounds second)
    {
        bool xOverlaps = first.min.x <= second.max.x && first.max.x >= second.min.x;
        bool yOverlaps = first.min.y <= second.max.y && first.max.y >= second.min.y;
        return xOverlaps && yOverlaps;
    }
}
