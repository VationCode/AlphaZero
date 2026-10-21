using alpha.ingame.input;
using alpha.ingame.item;
using UnityEngine;

namespace alpha.ingame.player
{
    /// <summary>장착 무기와 공격 후 전투 자세 유지 시간을 관리한다.</summary>
    public class CombatModule : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _readyDuration = 1.5f;

        private PlayerCore _playerCore;
        private float _readyUntil;

        public GunWeapon CurrentGun => _playerCore?.EquipmentModule?.CurrentWeapon?.GetComponent<GunWeapon>();
        public bool HasReadyTime => Time.time < _readyUntil;
        public bool CanAttack
        {
            get
            {
                AlphaInput input = AlphaInput.Instance;
                GunWeapon gun = CurrentGun;
                return gun != null && gun.IsEquipped && input != null &&
                       !input.IsQuickSlotOpen && !input.IsQuickSlotPressed;
            }
        }

        public bool CanContinueAttack
        {
            get
            {
                AlphaInput input = AlphaInput.Instance;
                GunWeapon gun = CurrentGun;
                return gun != null && gun.IsEquipped && input != null && !input.IsQuickSlotOpen;
            }
        }

        public bool IsCombatStanceActive
        {
            get
            {
                AlphaInput input = AlphaInput.Instance;
                CombatStateFlow flow = _playerCore?.CombatStateFlow;
                return input != null && !input.IsQuickSlotOpen && flow != null &&
                       (!input.IsQuickSlotPressed || flow.CurrentState == ECombatState.Attack) &&
                       flow.IsCombatStanceActive;
            }
        }

        private void Awake()
        {
            _playerCore = GetComponentInParent<PlayerCore>(true);
        }

        public void RegisterSuccessfulAttack(GunWeapon gun)
        {
            if (gun == null || gun != CurrentGun) return;
            // 성공한 마지막 발사를 기준으로 Ready 종료 시각을 다시 계산한다.
            _readyUntil = Time.time + _readyDuration;
        }

        public void ClearReady() => _readyUntil = 0f;
    }
}
