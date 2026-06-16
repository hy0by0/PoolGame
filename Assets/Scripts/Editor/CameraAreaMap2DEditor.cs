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
            if (GUILayout.Button("Collect"))
            {
                areaMap.CollectAreasFromChildrenMenu();
            }

            if (GUILayout.Button("Snap"))
            {
                areaMap.SnapChildrenToGrid();
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Rename"))
            {
                areaMap.RenameChildrenFromGrid();
            }

            if (GUILayout.Button("Create Right"))
            {
                areaMap.CreateAreaToRight();
            }
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
