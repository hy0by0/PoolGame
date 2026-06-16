using UnityEngine;

// 水領域のColliderを管理し、水面の高さを外部へ渡すクラス。
[DisallowMultipleComponent]
public sealed class WaterVolume2D : MonoBehaviour
{
    [Header("Inspector References")]
    [SerializeField] private Collider2D waterCollider;

    public Bounds VolumeBounds => waterCollider.bounds;
    public float SurfaceY => waterCollider.bounds.max.y;

    // 水は通り抜ける判定として使うため、起動時にTriggerへ揃える。
    private void Awake()
    {
        waterCollider.isTrigger = true;
    }

    // コンポーネント追加直後に、自分のColliderを自動で入れるための補助。
    private void Reset()
    {
        waterCollider = GetComponent<Collider2D>();
        waterCollider.isTrigger = true;
    }
}
