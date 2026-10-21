using System.Collections.Generic;
using alpha.ingame.input;
using UnityEngine;

namespace alpha.ingame.player
{
    public enum ELocomotionState
    {
        Idle,
        Move,
        Jump,
        Fall,
        Land,
        Mobility
    }

    public class LocomotionStateFlow : MonoBehaviour
    {
        private PlayerCore _playerCore;
        private Dictionary<ELocomotionState, LocomotionState> _stateDict;

        private LocomotionState _currentState;

        public ELocomotionState CurrentState { get; private set; } = ELocomotionState.Idle;

        private void Awake()
        {
            _playerCore = GetComponentInParent<PlayerCore>(true);
        }

        private void Start()
        {
            _stateDict = new Dictionary<ELocomotionState, LocomotionState>
            {
                { ELocomotionState.Idle, new IdleState(_playerCore) },
                { ELocomotionState.Move, new MoveState(_playerCore) },
                { ELocomotionState.Jump, new JumpState(_playerCore) },
                { ELocomotionState.Fall, new FallState(_playerCore) },
                { ELocomotionState.Land, new LandState(_playerCore) },
                { ELocomotionState.Mobility, new MobilityState(_playerCore) }
            };
            ChangeState(ELocomotionState.Idle);
        }

        private void Update()
        {
            _currentState?.UpdateState();
        }

        public void ChangeState(ELocomotionState newState)
        {
            if (_currentState == _stateDict[newState]) return;

            // 공중 및 착지 상태에 들어가기 전에 차징·연사와 Ready를 즉시 종료한다.
            if (newState == ELocomotionState.Jump ||
                newState == ELocomotionState.Fall ||
                newState == ELocomotionState.Land)
                _playerCore.CombatStateFlow?.CancelForAirState();

            _currentState?.ExitState();
            _currentState = _stateDict[newState];
            CurrentState = newState;
            _currentState?.EnterState();
        }

        public bool TryEnterMobility()
        {
            AlphaInput input = AlphaInput.Instance;
            if (input == null || !input.IsMobilityPressed ||
                _playerCore.CurrentArete == null || !_playerCore.LocomotionModule.IsGrounded)
                return false;

            // 아린처럼 기동이 정의되지 않은 아레테는 입력을 소비하지 않는다.
            if (!_playerCore.CurrentArete.CanUseMobility)
                return false;

            ChangeState(ELocomotionState.Mobility);
            return true;
        }
    }
}
