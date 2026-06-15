using UnityEngine;

[DisallowMultipleComponent]
public sealed class WaterVolume2D : MonoBehaviour
{
    [Header("Inspector References")]
    [SerializeField] private Collider2D waterCollider;

    public float SurfaceY => waterCollider.bounds.max.y;

    private void Awake()
    {
        waterCollider.isTrigger = true;
    }

    private void Reset()
    {
        waterCollider = GetComponent<Collider2D>();
        waterCollider.isTrigger = true;
    }

}
