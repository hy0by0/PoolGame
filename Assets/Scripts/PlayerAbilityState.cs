using UnityEngine;

// プレイヤーが現在使える能力をまとめて持つクラス。
[DisallowMultipleComponent]
public sealed class PlayerAbilityState : MonoBehaviour
{
    [Header("Abilities")]
    [SerializeField] private bool hasTerrainWaterSwitchAbility = true;

    public bool HasTerrainWaterSwitchAbility => hasTerrainWaterSwitchAbility;

    // アイテム取得などから、地形と水の切り替え能力を解放する。
    public void GrantTerrainWaterSwitchAbility()
    {
        hasTerrainWaterSwitchAbility = true;
    }
}
