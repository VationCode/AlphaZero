using UnityEngine;
using alpha.player.state;

namespace alpha.player.locomotion
{
    public class FlightState : PlayerStateBase
    {
        public override void Enter(PlayerCore playerCore)
        {
            var _anim = playerCore.AnimBoundary;
            _anim.FlightCrossFade();
        }
        public override void Update(PlayerCore playerCore)
        {
            var _loco = playerCore.LocomotionModule;
            var _anim = playerCore.AnimBoundary;
            var _input = playerCore.InputSystemBoundary;
            var _ctrl = playerCore.CharacterCtrlBoudary;
            var _state = playerCore.StateMachineFlow;

            // ==================== 연산
            _loco.HandleFlightMove(_input.MoveInputDir);
            _loco.HandleFlightRotation();
            Vector3 _finalVelocity = _loco.GetFinalVelocity();
            Vector3 _horizontal = new Vector3(_finalVelocity.x, 0, _finalVelocity.z);

            // ==================== 적용
            // 실제 이동
            _ctrl.SetMove(_finalVelocity);
            _anim.FlightAnim(_horizontal);

            if(_input.IsFlyInput)
            {
                _state.ChangeLocoState(ELocomotionStateType.Fall);
            }
        }

        public override void Exit(PlayerCore playerCore)
        {

        }
    }
}
