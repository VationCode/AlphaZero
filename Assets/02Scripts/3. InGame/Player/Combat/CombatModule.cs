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
        private EquippableWeapon _secondaryWeapon;
        private WeaponSecondaryAction _activeSecondary;

        public GunWeapon CurrentGun => _playerCore?.EquipmentModule?.CurrentWeapon?.GetComponent<GunWeapon>();
        public bool HasReadyTime => Time.time < _readyUntil;
        // 실제 Aim/Scope View가 활성화된 동안에만 부스터를 막는다.
        public bool IsAimOrScopeActive =>
            _activeSecondary is SecondaryViewAction view && view.IsActive;
        public bool CanAttack
        {
            get
            {
                AlphaInput input = AlphaInput.Instance;
                GunWeapon gun = CurrentGun;
                return gun != null && gun.IsEquipped && input != null &&
                       !input.IsQuickSlotOpen && !input.IsQuickSlotPressed &&
                       CanUseCombatFromLocomotion(input);
            }
        }

        public bool CanContinueAttack
        {
            get
            {
                AlphaInput input = AlphaInput.Instance;
                GunWeapon gun = CurrentGun;
                return gun != null && gun.IsEquipped && input != null &&
                       !input.IsQuickSlotOpen && CanUseCombatFromLocomotion(input);
            }
        }

        public bool IsCombatStanceActive
        {
            get
            {
                AlphaInput input = AlphaInput.Instance;
                CombatStateFlow flow = _playerCore?.CombatStateFlow;
                bool isViewActive = _activeSecondary is SecondaryViewAction view && view.IsActive;
                return input != null && !input.IsQuickSlotOpen && flow != null &&
                       CanUseCombatFromLocomotion(input) &&
                       (!input.IsQuickSlotPressed || flow.CurrentState == ECombatState.Attack) &&
                       (flow.IsCombatStanceActive || isViewActive);
            }
        }

        private void Awake()
        {
            _playerCore = GetComponentInParent<PlayerCore>(true);
        }

        private void Update()
        {
            AlphaInput input = AlphaInput.Instance;
            EquippableWeapon weapon = _playerCore?.EquipmentModule?.CurrentWeapon?.GetComponent<EquippableWeapon>();
            bool available = input != null && !input.IsQuickSlotOpen && !input.IsQuickSlotPressed &&
                             weapon != null && weapon.IsEquipped && CanUseCombatFromLocomotion(input);

            if (_activeSecondary != null)
            {
                if (!available || weapon != _secondaryWeapon || weapon.SecondaryAction != _activeSecondary)
                {
                    CancelSecondary();
                    return;
                }

                if (input.IsSecondaryReleased || !input.IsSecondaryHeld)
                {
                    WeaponSecondaryAction action = _activeSecondary;
                    _activeSecondary = null;
                    _secondaryWeapon = null;
                    action.End();
                    return;
                }

                _activeSecondary.Tick();
                return;
            }

            if (!available || !input.IsSecondaryPressed || weapon.SecondaryAction == null) return;
            _secondaryWeapon = weapon;
            _activeSecondary = weapon.SecondaryAction;
            _activeSecondary.Begin();

            // 부스터 중 Aim/Scope가 켜지면 일반 전투 이동으로 전환한다.
            LocomotionStateFlow flow = _playerCore.LocomotionStateFlow;
            if (IsAimOrScopeActive && flow != null && flow.CurrentState == ELocomotionState.Mobility)
                flow.ChangeState(ELocomotionState.Move);
        }

        private void OnDisable() => CancelSecondary();

        public void CancelSecondary()
        {
            WeaponSecondaryAction action = _activeSecondary;
            _activeSecondary = null;
            _secondaryWeapon = null;
            action?.Cancel();
        }

        // Primary와 Secondary 모두 공중 및 착지 중에는 사용할 수 없다.
        private bool CanUseCombatFromLocomotion(AlphaInput input)
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
