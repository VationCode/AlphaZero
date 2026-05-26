using alpha.Item;
using System;
using UnityEngine;

namespace alpha.ingame.Item.weapon
{

    public enum EWeaponType
    {
        Melee = 0,
        Range = 1,
        Special = 2
    }

    public enum EWeaponHandType
    {
        OneHand,
        TwoHand,
        DualWield
    }

    public enum EWeaponHolderType
    {
        Right,
        Left,
    }

    [Serializable]
    public struct EWeaponData
    {
        public GameObject itemObj;
        public EWeaponHolderType WeaponHolderType;
    }

    [CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/Item/Weapon")]
    public class WeaponDataSO : ItemDataSO
    {
        [Header("[ Weapon ]")]
        public EWeaponType WeaponType;
        public EWeaponHandType WeaponHandType;
        public EWeaponData[] WeaponDatas;
    }
}