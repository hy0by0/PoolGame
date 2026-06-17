using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraAreaMap2D))]
public sealed class CameraAreaMap2DEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CameraAreaMap2D areaMap = (CameraAreaMap2D)target;

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Area Tools", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button(new GUIContent("Collect", "子オブジェクトからCameraArea2Dを集めて一覧を更新します。")))
            {
                areaMap.CollectAreasFromChildrenMenu();
            }

            if (GUILayout.Button(new GUIContent("Snap", "各エリアをGrid PositionとArea Sizeに合わせた位置へ移動します。")))
            {
                areaMap.SnapChildrenToGrid();
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button(new GUIContent("Rename", "各エリアのArea IdとGameObject名をGrid Positionから付け直します。")))
            {
                areaMap.RenameChildrenFromGrid();
            }

            if (GUILayout.Button(new GUIContent("Create Right", "既存エリアの右端のさらに右へ、新しいエリアを作成します。")))
            {
                areaMap.CreateAreaToRight();
            }
        }

        if (GUILayout.Button(new GUIContent("Create At Grid", "Create AreaのNew Area Grid Positionで指定した座標に新しいエリアを作成します。")))
        {
            areaMap.CreateAreaAtGrid();
        }

        EditorGUILayout.Space(6f);
        CameraArea2D[] areas = areaMap.Areas;
        if (areas == null)
        {
            EditorGUILayout.LabelField("Managed Areas: 0");
            return;
        }

        EditorGUILayout.LabelField($"Managed Areas: {areas.Length}");

        for (int i = 0; i < areas.Length; i++)
        {
            CameraArea2D area = areas[i];
            if (area == null)
            {
                EditorGUILayout.LabelField($"[{i}] Missing");
                continue;
            }

            EditorGUILayout.ObjectField(
                $"[{i}] {area.AreaId} {area.GridPosition}",
                area,
                typeof(CameraArea2D),
                true);
        }
    }
}
