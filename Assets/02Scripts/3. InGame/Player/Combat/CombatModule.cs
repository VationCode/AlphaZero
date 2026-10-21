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
                       !input.IsQuickSlotOpen && !input.IsQuickSlotPressed &&
                       CanAttackFromLocomotion(input);
            }
        }

        public bool CanContinueAttack
        {
            get
            {
                AlphaInput input = AlphaInput.Instance;
                GunWeapon gun = CurrentGun;
                return gun != null && gun.IsEquipped && input != null &&
                       !input.IsQuickSlotOpen && CanAttackFromLocomotion(input);
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

        private bool CanAttackFromLocomotion(AlphaInput input)
        {
            LocomotionModule locomotion = _playerCore?.LocomotionModule;
            LocomotionStateFlow flow = _playerCore?.LocomotionStateFlow;
            if (locomotion == null || flow == null || !locomotion.IsGrounded)
                return false;

            ELocomotionState state = flow.CurrentState;
            // 점프 입력과 공격 입력이 같은 프레임에 들어오면 점프를 우선한다.
            return state != ELocomotionState.Jump &&
                   state != ELocomotionState.Fall &&
                   state != ELocomotionState.Land &&
                   !(input.IsJump && locomotion.CanGroundJump);
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
