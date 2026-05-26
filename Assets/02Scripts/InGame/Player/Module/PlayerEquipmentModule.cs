using alpha.ingame.Item.weapon;
using System;
using UnityEngine;

namespace alpha.player.equipment
{
    public class PlayerEquipmentModule : MonoBehaviour
    {
        [SerializeField]
        private EquipmentSlotData[] m_weaponSlotsL;
        [SerializeField]
        private EquipmentSlotData[] m_weaponSlotsR;

        [Tooltip("임시 데이터"), SerializeField]
        private WeaponDataSO[] m_weaponDatas;   // 임시로 직접 가지고 있는 상태로 진행, 이후 인벤토리 시스템과 연동하여 장착한 무기 데이터 받아오기

        #region RunTime
        private WeaponDataSO m_currentWeaponData;
        private int m_currentWeaponIndex;
        #endregion

        private void Start()
        {
            m_currentWeaponIndex = 0;

            for(int i = 0; i < m_weaponDatas.Length; i++)
            {
                OnEquip(m_weaponDatas[i]);
            }

            OnSwap(0);
        }

        public int GetCurrentSwapNum()
        {
            return m_currentWeaponIndex;
        }
        public void SetSwapNum(int p_swapNum)
        {
            m_currentWeaponIndex = p_swapNum;
        }

        public void OnSwap(int p_swapNum)
        {
            if (m_weaponSlotsR[p_swapNum].WeaponData == null) return;

            for (int i = 0; i < m_weaponSlotsR.Length; i++)
            {
                m_weaponSlotsL[i].gameObject.SetActive(false);
                m_weaponSlotsR[i].gameObject.SetActive(false);
            }

            m_weaponSlotsR[p_swapNum].gameObject.SetActive(true);

            m_currentWeaponData = m_weaponSlotsR[p_swapNum].WeaponData;

            if (m_weaponSlotsL[p_swapNum].WeaponData != null)
            {
                m_weaponSlotsL[p_swapNum].gameObject.SetActive(true);
            }
        }

        public void OnEquip(WeaponDataSO p_weaponData)
        {
            // WeaponDatas의 길이는 무기 종류에 따라 달라질 수 있음 (예: 양손 무기는 1개, 듀얼은 2개)
            int slotIndex = (int)p_weaponData.WeaponType;

            ClearSlot(slotIndex);

            for (int i = 0; i < p_weaponData.WeaponDatas.Length; i++)
            {
                var weaponData = p_weaponData.WeaponDatas[i];

                if (weaponData.WeaponHolderType == EWeaponHolderType.Left)
                {
                    EquipToSlot(m_weaponSlotsL[slotIndex],weaponData.itemObj,p_weaponData);
                }
                else
                {
                    EquipToSlot(m_weaponSlotsR[slotIndex],weaponData.itemObj,p_weaponData);
                }
            }
        }

        private void EquipToSlot(EquipmentSlotData p_slot, GameObject p_weaponPrefab, WeaponDataSO p_weaponData)
        {
            if (p_slot.transform.childCount > 0)
            {
                Destroy(p_slot.transform.GetChild(0).gameObject);
            }

            var weaponObj = Instantiate(p_weaponPrefab);

            weaponObj.transform.SetParent(p_slot.transform);
            weaponObj.transform.localPosition = Vector3.zero;
            weaponObj.transform.localRotation = Quaternion.identity;
            weaponObj.transform.localScale = Vector3.one;

            p_slot.WeaponData = p_weaponData;
        }

        private void ClearSlot(int p_slotIndex)
        {
            if (m_weaponSlotsL[p_slotIndex].transform.childCount > 0)
            {
                Destroy(m_weaponSlotsL[p_slotIndex].transform.GetChild(0).gameObject);

                m_weaponSlotsL[p_slotIndex].WeaponData = null;
            }

            if (m_weaponSlotsR[p_slotIndex].transform.childCount > 0)
            {
                Destroy(m_weaponSlotsR[p_slotIndex].transform.GetChild(0).gameObject);

                m_weaponSlotsR[p_slotIndex].WeaponData = null;
            }
        }
    }
}