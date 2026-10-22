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
        private GunWeapon _knownGun;

        public ECombatState CurrentState { get; private set; } = ECombatState.Idle;
        public bool IsCombatStanceActive => _currentState != null && CurrentState != ECombatState.Idle;

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
                ChangeState(ECombatState.Idle);
                _playerCore.CombatModule.ClearReady();
                return;
            }

            GunWeapon gun = _playerCore.CombatModule.CurrentGun;
            if (gun != _knownGun || input.IsQuickSlotOpen ||
                _playerCore.SkillStateFlow?.IsExecuting == true ||
                (input.IsQuickSlotPressed && CurrentState != ECombatState.Attack))
            {
                // 무기 교체와 퀵슬롯은 진행 중인 공격과 남은 자세 시간을 초기화한다.
                ChangeState(ECombatState.Idle);
                _playerCore.CombatModule.ClearReady();
                _knownGun = gun;
                return;
            }

            _currentState.UpdateState();
        }

        private void OnEnable()
        {
            if (_states != null && _currentState == null)
                ChangeState(ECombatState.Idle);
        }

        private void OnDisable()
        {
            _currentState?.ExitState();
            _currentState = null;
            CurrentState = ECombatState.Idle;
            if (_playerCore != null)
                _playerCore.CombatModule?.ClearReady();
        }

        public void CancelForAirState()
        {
            // 공격 상태를 벗어나면 GunWeapon.CancelAttack이 차징을 발사 없이 종료한다.
            if (CurrentState != ECombatState.Idle)
                ChangeState(ECombatState.Idle);
            _playerCore?.CombatModule?.ClearReady();
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
