using UnityEngine;

namespace alpha.ingame.player
{
    public class EquipmentManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] _weaponHolder;

        [SerializeField]
        private GameObject[] _weaponPrefabs;

        public int AreteNum { get; private set; }
        public GameObject CurrentWeapon { get; private set; }

        public void HandlePlayerSetup(EAreteType type)
        {
            SetAreteNum((int)type);
            CreateWeapon((int)type);
        }

        public void SetAreteNum(int p_num)
        {
            AreteNum = p_num;
        }

        public void CreateWeapon(int p_num)
        {
            
            CurrentWeapon = Instantiate(_weaponPrefabs[p_num], _weaponHolder[p_num].transform);
        }

    }
}