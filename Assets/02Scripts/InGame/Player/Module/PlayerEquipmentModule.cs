using System;
using UnityEngine;

namespace alpha.player.equipment
{
    [Serializable]
    public struct PlayerWeaponData
    {
        public GameObject[] Weapons;
        public int Index;
    }

    public class PlayerEquipmentModule : MonoBehaviour
    {
        [SerializeField]
        private PlayerWeaponData[] m_weapons;

        #region RunTime
        private int m_currentWeaponIndex;
        #endregion

        private void Start()
        {
            m_currentWeaponIndex = 0;

            for(int i = 0; i < m_weapons.Length; i++)
            {
                if(i == m_currentWeaponIndex)
                {
                    for(int j = 0; j < m_weapons[i].Weapons.Length; j++)
                    {
                        m_weapons[i].Weapons[j].SetActive(true);
                    }
                }
                else
                {
                    for (int j = 0; j < m_weapons[i].Weapons.Length; j++)
                    {
                        m_weapons[i].Weapons[j].SetActive(false);
                    }
                }
            }
        }

        public void OnSwap(int swapNum)
        {

            m_currentWeaponIndex = swapNum;
        }
    }
}