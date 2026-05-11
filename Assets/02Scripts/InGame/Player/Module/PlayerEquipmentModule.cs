using UnityEngine;

namespace alpha.player.equipment
{
    public class PlayerEquipmentModule : MonoBehaviour
    {
        [SerializeField]
        private Transform m_weaponHolderL;
        [SerializeField]
        private Transform m_weaponHolderR;

        #region RunTime
        private int m_currentWeaponIndex;
        #endregion

        public void OnSwap(int swapNum)
        {

            m_currentWeaponIndex = swapNum;
        }
    }
}