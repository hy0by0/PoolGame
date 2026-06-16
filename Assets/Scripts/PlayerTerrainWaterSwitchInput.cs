using UnityEngine;
using UnityEngine.InputSystem;

// プレイヤー入力から、地形と水の切り替え能力を実行するクラス。
[DisallowMultipleComponent]
public sealed class PlayerTerrainWaterSwitchInput : MonoBehaviour
{
    [Header("Inspector References")]
    [SerializeField] private PlayerAbilityState abilityState;
    [SerializeField] private RoomTerrainWaterSwitcher2D roomSwitcher;

    // Input System の SwitchTerrainWater アクションから切り替え入力を受け取る。
    public void OnSwitchTerrainWater(InputValue value)
    {
        if (value.isPressed && abilityState.HasTerrainWaterSwitchAbility)
        {
            roomSwitcher.ToggleCurrentCameraArea();
        }
    }

    // コンポーネント追加直後に、同じオブジェクト上の能力参照を自動で入れるための補助。
    private void Reset()
    {
        abilityState = GetComponent<PlayerAbilityState>();
    }
}
