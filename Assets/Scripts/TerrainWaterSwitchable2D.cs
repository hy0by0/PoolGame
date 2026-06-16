using UnityEngine;

// 同じ形状のまま、地形と水のふるまいを切り替える対象を扱うクラス。
[DisallowMultipleComponent]
public sealed class TerrainWaterSwitchable2D : MonoBehaviour
{
    [Header("Inspector References")]
    [SerializeField] private Collider2D switchCollider;
    [SerializeField] private WaterVolume2D waterVolume;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("State")]
    [SerializeField] private TerrainWaterState2D currentState = TerrainWaterState2D.Terrain;
    [SerializeField] private string groundLayerName = "Ground";
    [SerializeField] private string waterLayerName = "Water";
    [SerializeField] private Color terrainColor = Color.white;
    [SerializeField] private Color waterColor = new Color(0.25f, 0.65f, 1f, 0.55f);

    public Bounds SwitchBounds => switchCollider.bounds;

    private void Awake()
    {
        ApplyState(currentState);
    }

    // 現在の状態を反対側へ切り替える。
    public void ToggleState()
    {
        TerrainWaterState2D nextState = currentState == TerrainWaterState2D.Terrain
            ? TerrainWaterState2D.Water
            : TerrainWaterState2D.Terrain;

        ApplyState(nextState);
    }

    // 地形または水としてのLayer、Trigger、見た目をまとめて反映する。
    public void ApplyState(TerrainWaterState2D nextState)
    {
        currentState = nextState;
        bool waterActive = currentState == TerrainWaterState2D.Water;

        gameObject.layer = waterActive ? LayerMask.NameToLayer(waterLayerName) : LayerMask.NameToLayer(groundLayerName);
        switchCollider.isTrigger = waterActive;
        waterVolume.SetWaterActive(waterActive);
        spriteRenderer.color = waterActive ? waterColor : terrainColor;
    }

    // コンポーネント追加直後に、同じオブジェクト上の参照を自動で入れるための補助。
    private void Reset()
    {
        switchCollider = GetComponent<Collider2D>();
        waterVolume = GetComponent<WaterVolume2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
