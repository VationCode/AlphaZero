using System;
using alpha.ingame.input;
using alpha.ingame.item;
using UnityEngine;

namespace alpha.ingame.player
{
    /// <summary>장착 무기의 사용 조건과 보조 조작을 관리한다.</summary>
    public class CombatModule : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _readyDuration = 1.5f;

        private PlayerCore _playerCore;
        private EquippableWeapon _secondaryWeapon;
        private WeaponSecondaryAction _activeSecondary;

        public event Action ShotFired;

        public GunWeapon CurrentGun => _playerCore?.EquipmentModule?.CurrentWeapon?.GetComponent<GunWeapon>();
        public float ReadyDuration => _readyDuration;
        // 실제 Aim/Scope View가 활성화된 동안에만 부스터를 막는다.
        public bool IsAimOrScopeActive =>
            _activeSecondary is SecondaryViewAction view && view.IsActive;
        public bool CanReload
        {
            get
            {
                AlphaInput input = AlphaInput.Instance;
                GunWeapon gun = CurrentGun;
                return gun != null && gun.CanReload && input != null &&
                       !input.IsQuickSlotOpen && !input.IsQuickSlotPressed &&
                       _playerCore?.SkillStateFlow?.IsExecuting != true &&
                       CanUseCombatFromLocomotion(input);
            }
        }
        public bool CanAttack
        {
            get
            {
                AlphaInput input = AlphaInput.Instance;
                GunWeapon gun = CurrentGun;
                return gun != null && gun.IsEquipped && !gun.IsReloading &&
                       _playerCore?.CombatActionFlow?.CurrentState != ECombatAction.Reload &&
                       _playerCore?.CombatActionFlow?.CurrentState != ECombatAction.Skill &&
                       input != null &&
                       !input.IsQuickSlotOpen && !input.IsQuickSlotPressed &&
                       _playerCore?.SkillStateFlow?.IsExecuting != true &&
                       _playerCore?.SkillStateFlow?.IsPrimaryConsumedThisFrame != true &&
                       CanUseCombatFromLocomotion(input);
            }
        }

        public bool CanContinueAttack
        {
            get
            {
                AlphaInput input = AlphaInput.Instance;
                GunWeapon gun = CurrentGun;
                return gun != null && gun.IsEquipped && !gun.IsReloading &&
                       _playerCore?.CombatActionFlow?.CurrentState == ECombatAction.Attack && input != null &&
                       !input.IsQuickSlotOpen && _playerCore?.SkillStateFlow?.IsExecuting != true &&
                       _playerCore?.SkillStateFlow?.IsPrimaryConsumedThisFrame != true &&
                       CanUseCombatFromLocomotion(input);
            }
        }

        public bool IsCombatStanceActive
        {
            get
            {
                AlphaInput input = AlphaInput.Instance;
                CombatActionFlow action = _playerCore?.CombatActionFlow;
                CombatStanceFlow stance = _playerCore?.CombatStanceFlow;
                return input != null && !input.IsQuickSlotOpen && stance != null &&
                       _playerCore?.SkillStateFlow?.IsExecuting != true &&
                       CanUseCombatFromLocomotion(input) &&
                       (!input.IsQuickSlotPressed || action?.IsAttacking == true) &&
                       stance.IsCombat;
            }
        }

        public bool ShouldFaceAimDirection => IsCombatStanceActive;

        private void Awake()
        {
            _playerCore = GetComponentInParent<PlayerCore>(true);
        }

        private void Update()
        {
            AlphaInput input = AlphaInput.Instance;
            EquippableWeapon weapon = _playerCore?.EquipmentModule?.CurrentWeapon?.GetComponent<EquippableWeapon>();
            bool available = input != null && !input.IsQuickSlotOpen && !input.IsQuickSlotPressed &&
                             weapon != null && weapon.IsEquipped &&
                             _playerCore?.SkillStateFlow?.IsExecuting != true &&
                             CanUseCombatFromLocomotion(input);

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

        public void RegisterSuccessfulAttack(GunWeapon gun, bool playFireRecoil = false)
        {
            if (gun == null || gun != CurrentGun) return;
            // 성공한 마지막 발사를 기준으로 Ready 종료 시각을 다시 계산한다.
            _playerCore?.CombatStanceFlow?.RefreshReady();
            if (playFireRecoil) ShotFired?.Invoke();
        }
    }
}
