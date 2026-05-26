using alpha.ingame.Item.weapon;
using Unity.Collections;
using UnityEngine;

namespace alpha.player.equipment
{
    public class EquipmentSlotData : MonoBehaviour
    {
        public int SlotIndex;

        [ReadOnly]
        public WeaponDataSO WeaponData;
    }
}