using System.Collections.Generic;
using alpha.ingame.input;
using alpha.ingame.item;
using UnityEngine;

namespace alpha.ingame.player
{
    public enum ECombatState
    {
        Idle,
        Attack,
        Ready
    }

    /// <summary>이동 상태와 별개로 공격과 공격 후 자세를 전환한다.</summary>
    [DefaultExecutionOrder(-100)]
    public class CombatStateFlow : MonoBehaviour
    {
        private PlayerCore _playerCore;
        private Dictionary<ECombatState, CombatState> _states;
        private CombatState _currentState;
        private CombatReloadState _reloadState;
        private GunWeapon _knownGun;

        public ECombatState CurrentState { get; private set; } = ECombatState.Idle;
        public bool IsCombatStanceActive => _currentState != null && CurrentState != ECombatState.Idle;
        public bool IsReloading => _reloadState?.IsActive == true;

        private void Awake()
        {
            _playerCore = GetComponentInParent<PlayerCore>(true);
        }

        private void Start()
        {
            _states = new Dictionary<ECombatState, CombatState>
            {
                { ECombatState.Idle, new CombatIdleState(_playerCore) },
                { ECombatState.Attack, new CombatAttackState(_playerCore) },
                { ECombatState.Ready, new CombatReadyState(_playerCore) }
            };
            _reloadState = new CombatReloadState(_playerCore);
            _knownGun = _playerCore.CombatModule.CurrentGun;
            ChangeState(ECombatState.Idle);
        }

        private void Update()
        {
            if (_currentState == null) return;

            AlphaInput input = AlphaInput.Instance;
            if (input == null)
            {
                // 입력 객체가 사라졌을 때 차징 종료를 발사로 해석하지 않는다.
                _reloadState.Cancel();
                ChangeState(ECombatState.Idle);
                _playerCore.CombatModule.ClearReady();
                return;
            }

            GunWeapon gun = _playerCore.CombatModule.CurrentGun;
            if (gun != _knownGun || input.IsQuickSlotOpen ||
                _playerCore.SkillStateFlow?.IsExecuting == true ||
                (input.IsQuickSlotPressed && CurrentState != ECombatState.Attack))
            {
                // 무기 교체와 퀵슬롯은 진행 중인 공격·재장전과 남은 자세 시간을 초기화한다.
                _reloadState.Cancel();
                ChangeState(ECombatState.Idle);
                _playerCore.CombatModule.ClearReady();
                _knownGun = gun;
                return;
            }

            if (input.IsReload && _playerCore.CombatModule.CanReload)
            {
                // 공격을 종료하되 전투 자세는 재장전 동작과 함께 유지한다.
                BeginReload();
                return;
            }

            if (_reloadState.UpdateState())
                _playerCore.CombatModule.StartReadyTime();

            _currentState.UpdateState();

            // 마지막 탄환을 발사한 프레임에 재장전 상태로 전환한다.
            if (gun != null && gun.CurrentAmmo == 0 && _playerCore.CombatModule.CanReload)
            {
                BeginReload();
            }
        }

        private void OnEnable()
        {
            if (_states != null && _currentState == null)
                ChangeState(ECombatState.Idle);
        }

        private void OnDisable()
        {
            _reloadState?.Cancel();
            _currentState?.ExitState();
            _currentState = null;
            CurrentState = ECombatState.Idle;
            if (_playerCore != null)
                _playerCore.CombatModule?.ClearReady();
        }

        public void CancelForAirState()
        {
            // 공격·재장전 상태를 벗어나면 해당 무기의 진행 중인 동작을 취소한다.
            _reloadState?.Cancel();
            if (CurrentState != ECombatState.Idle)
                ChangeState(ECombatState.Idle);
            _playerCore?.CombatModule?.ClearReady();
        }

        private void BeginReload()
        {
            // Reload는 별도 동작으로 실행하고 기본 자세는 Ready에 둔다.
            ChangeState(ECombatState.Ready);
            if (!_reloadState.TryEnter())
                ChangeState(_playerCore.CombatModule.HasReadyTime ? ECombatState.Ready : ECombatState.Idle);
        }

        public void ChangeState(ECombatState nextState)
        {
            if (_states == null || !_states.TryGetValue(nextState, out CombatState next) || _currentState == next)
                return;

            _currentState?.ExitState();
            _currentState = next;
            CurrentState = nextState;
            _currentState.EnterState();
        }
    }
}
