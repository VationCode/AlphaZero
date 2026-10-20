using System;
using UnityEngine;

namespace alpha.ingame.player
{
    public enum EEquipmentSlot
    {
        Main=0,
        Sub = 1
    }

    public class EquipmentModule : MonoBehaviour
    {
        [Serializable]
        private struct StartingWeapons
        {
            [SerializeField] private EAreteType _areteType;
            [SerializeField] private GameObject _mainPrefab;
            [SerializeField] private GameObject _subPrefab;

            public EAreteType AreteType => _areteType;
            public GameObject MainPrefab => _mainPrefab;
            public GameObject SubPrefab => _subPrefab;
        }

        [SerializeField] private StartingWeapons[] _startingWeapons;

        public GameObject MainWeapon { get; private set; }
        public GameObject SubWeapon { get; private set; }
        public GameObject CurrentWeapon { get; private set; }
        public EEquipmentSlot ActiveSlot { get; private set; } = EEquipmentSlot.Main;

        public void Initialize(EAreteType areteType, GameObject model)
        {
            MainWeapon = null;
            SubWeapon = null;
            CurrentWeapon = null;
            ActiveSlot = EEquipmentSlot.Main;
            if (model == null || _startingWeapons == null) return;

            foreach (StartingWeapons entry in _startingWeapons)
            {
                if (entry.AreteType != areteType) continue;

                Transform holder = FindHolder(model.transform);
                if (holder == null)
                {
                    Debug.LogWarning($"{nameof(EquipmentModule)}: {areteType} 모델에 WeaponHolder가 없습니다.", this);
                    return;
                }

                // 두 무기를 함께 생성하되 Sub는 처음부터 비활성화한다.
                bool holderWasActive = holder.gameObject.activeSelf;
                holder.gameObject.SetActive(false);

                MainWeapon = CreateWeapon(entry.MainPrefab, holder);
                SubWeapon = CreateWeapon(entry.SubPrefab, holder);

                if (SubWeapon != null) SubWeapon.SetActive(false);
                if (MainWeapon != null) MainWeapon.SetActive(true);
                holder.gameObject.SetActive(holderWasActive);

                ActiveSlot = EEquipmentSlot.Main;
                CurrentWeapon = MainWeapon;
                return;
            }
        }

        public bool SelectSlot(EEquipmentSlot slot)
        {
            if (slot != EEquipmentSlot.Main && slot != EEquipmentSlot.Sub) return false;
            GameObject selected = slot == EEquipmentSlot.Main ? MainWeapon : SubWeapon;
            if (selected == null) return false;

            // 퀵슬롯 UI는 이 메서드만 호출하면 된다.
            if (MainWeapon != null) MainWeapon.SetActive(slot == EEquipmentSlot.Main);
            if (SubWeapon != null) SubWeapon.SetActive(slot == EEquipmentSlot.Sub);
            ActiveSlot = slot;
            CurrentWeapon = selected;
            return true;
        }

        private static GameObject CreateWeapon(GameObject prefab, Transform holder)
        {
            if (prefab == null) return null;
            GameObject weapon = Instantiate(prefab, holder, false);
            weapon.transform.localPosition = prefab.transform.localPosition;
            weapon.transform.localRotation = prefab.transform.localRotation;
            weapon.transform.localScale = prefab.transform.localScale;
            return weapon;
        }

        private static Transform FindHolder(Transform root)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == "WeaponHolder") return child;
            return null;
        }
    }
}
