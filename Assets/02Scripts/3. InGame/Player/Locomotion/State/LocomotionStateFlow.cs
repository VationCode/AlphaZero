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

            _currentState?.ExitState();
            _currentState = _stateDict[newState];
            _currentState?.EnterState();
        }

        public bool TryEnterMobility()
        {
            AlphaInput input = AlphaInput.Instance;
            if (input == null || !input.IsMobilityPressed ||
                _playerCore.CurrentModel == null || !_playerCore.LocomotionModule.IsGrounded)
                return false;

            // 아린은 기동 방식이 정해질 때까지 입력을 소비하지 않는다.
            EAreteType areteType = _playerCore.CurrentAreteType;
            if (areteType != EAreteType.Heruna && areteType != EAreteType.Silyon)
                return false;

            ChangeState(ELocomotionState.Mobility);
            return true;
        }
    }
}
