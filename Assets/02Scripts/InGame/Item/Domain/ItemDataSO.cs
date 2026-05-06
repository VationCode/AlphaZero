// Domain : 데이터 / 개념
using UnityEngine;

namespace alpha.Item
{
    public enum EItemType
    {
        Weapon,
        Armor,
        Consumable,     // 소모품
        Material,       // 강화재료
        Quest
    }
    public class ItemDataSO : ScriptableObject
    {
        public string ItemName;
        public EItemType ItemType;
        public GameObject itemObj;
    }
}