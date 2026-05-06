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
        private GameObject m_currentWeapon;
        #endregion

        public void OnSwap(int swapNum)
        {

        }
    }
}