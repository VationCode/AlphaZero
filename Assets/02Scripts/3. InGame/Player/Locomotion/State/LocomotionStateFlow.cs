using System.Collections.Generic;
using UnityEngine;

namespace alpha.ingame.player
{
    public enum ELocomotionState
    {
        Idle,
        Move,
        Turn,
        Jump,
        Fall,
        Land,
        Dash
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
                { ELocomotionState.Turn, new TurnState(_playerCore) },
                { ELocomotionState.Jump, new JumpState(_playerCore) },
                { ELocomotionState.Fall, new FallState(_playerCore) },
                { ELocomotionState.Land, new LandState(_playerCore) },
                { ELocomotionState.Dash, new DashState(_playerCore) }
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
    }
}