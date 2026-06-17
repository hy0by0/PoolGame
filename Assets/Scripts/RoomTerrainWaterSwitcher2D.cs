using System;
using UnityEngine;

// プレイヤーがいるエリア内の地形と水だけをまとめて切り替えるクラス。
[DisallowMultipleComponent]
public sealed class RoomTerrainWaterSwitcher2D : MonoBehaviour
{
    [Header("Inspector References")]
    [Tooltip("現在プレイヤーがいるCameraArea2Dを取得するためのコントローラです。")]
    [SerializeField] private AreaCameraController2D areaCameraController;

    private TerrainWaterSwitchable2D[] activeSwitchables = Array.Empty<TerrainWaterSwitchable2D>();
    private CameraArea2D activeArea;

    private void Awake()
    {
        RefreshCurrentAreaTargets();
    }

    private void LateUpdate()
    {
        RefreshAreaTargetsWhenAreaChanged();
    }

    // 現在プレイヤーがいるエリア内の切り替え対象だけを反転する。
    public void ToggleCurrentCameraArea()
    {
        RefreshAreaTargetsWhenAreaChanged();

        for (int i = 0; i < activeSwitchables.Length; i++)
        {
            activeSwitchables[i].ToggleState();
        }
    }

    // プレイヤーのエリアが変わっていたら、切り替え対象を集め直す。
    private void RefreshAreaTargetsWhenAreaChanged()
    {
        CameraArea2D nextArea = areaCameraController.CurrentArea;

        if (ReferenceEquals(nextArea, activeArea))
        {
            return;
        }

        activeArea = nextArea;
        RefreshCurrentAreaTargets();
    }

    // 現在エリア範囲と重なるTerrainWaterSwitchable2Dをシーンから自動収集する。
    public void RefreshCurrentAreaTargets()
    {
        Bounds areaBounds = areaCameraController.CurrentAreaBounds;
        TerrainWaterSwitchable2D[] allSwitchables = FindObjectsByType<TerrainWaterSwitchable2D>(FindObjectsSortMode.InstanceID);
        int activeCount = CountSwitchablesInsideArea(allSwitchables, areaBounds);
        TerrainWaterSwitchable2D[] nextSwitchables = new TerrainWaterSwitchable2D[activeCount];
        int nextIndex = 0;

        for (int i = 0; i < allSwitchables.Length; i++)
        {
            if (allSwitchables[i].IsInsideArea(areaBounds))
            {
                nextSwitchables[nextIndex] = allSwitchables[i];
                nextIndex++;
            }
        }

        activeSwitchables = nextSwitchables;
    }

    // 現在エリアに含まれる切り替え対象の数を数える。
    private static int CountSwitchablesInsideArea(TerrainWaterSwitchable2D[] allSwitchables, Bounds areaBounds)
    {
        int count = 0;

        for (int i = 0; i < allSwitchables.Length; i++)
        {
            if (allSwitchables[i].IsInsideArea(areaBounds))
            {
                count++;
            }
        }

        return count;
    }

    private void Reset()
    {
        areaCameraController = FindFirstObjectByType<AreaCameraController2D>();
    }
}
