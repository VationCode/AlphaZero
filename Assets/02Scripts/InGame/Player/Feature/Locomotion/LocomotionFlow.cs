using System.Collections.Generic;
using UnityEngine;

namespace alpha.player.locomotion
{
    public enum ELocomotionStateType
    {
        Idle,
        Move,
        Jump,
        Fall,
        Land,
        Dash,
        Dodge
    }

    public class LocomotionFlow : MonoBehaviour
    {
        private PlayerCore _core;

        public LocomotionState CurrentState => _currentState;
        private LocomotionState _currentState;

        public ELocomotionStateType CurrentStateType => _currentStateType;
        private ELocomotionStateType _currentStateType;

        private Dictionary<ELocomotionStateType, LocomotionState> _stateDict;

        public void Bind(PlayerCore p_core)
        {
            _core = p_core;
            Initialize();
        }

        private void Initialize()
        {
            _stateDict = new Dictionary<ELocomotionStateType, LocomotionState>
            {
                { ELocomotionStateType.Idle, new IdleState(_core) },
                { ELocomotionStateType.Move, new MoveState(_core) },
                //{ ELocomotionStateType.Jump, new JumpState(_core) },
                //{ ELocomotionStateType.Fall, new FallState(_core) },
                //{ ELocomotionStateType.Land, new LandState(_core) },
                { ELocomotionStateType.Dash, new DashState(_core) },
                { ELocomotionStateType.Dodge, new DodgeState(_core) }
            };

            ChangeState(ELocomotionStateType.Idle);
        }

        private void Update()
        {
            if(_core == null) return;
            _currentState?.Update();
        }

        public void ChangeState(ELocomotionStateType p_newStateType)
        {
            if(_currentState == null)
            {
                _currentState = _stateDict[p_newStateType];
                _currentStateType = p_newStateType;
                _currentState.Enter();
                return;
            }

            if (_currentStateType == p_newStateType) return;

            _currentState?.Exit();
            
            _currentState = _stateDict[p_newStateType];
            _currentStateType = p_newStateType;

            _currentState.Enter();
        }
    }
}
