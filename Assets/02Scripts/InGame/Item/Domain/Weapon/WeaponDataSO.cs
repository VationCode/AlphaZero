using alpha.Item;
using UnityEngine;

namespace alpha.ingame.Item.weapon
{
    public enum EWeaponType
    {
        Melee,
        Range,
        Special
    }

    public enum EHandType
    {
        OneHand,
        TwoHand
    }

    public class WeaponDataSO : ItemDataSO
    {
        [Header("[ Weapon ]")]
        public EWeaponType WeaponType;
        public EHandType HandType;
    }
}