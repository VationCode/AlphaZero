using UnityEngine;
using alpha.player.state;

namespace alpha.player.locomotion
{
    public class JumpUpState : PlayerStateBase
    {
        public override void Enter(PlayerCore playerCore)
        {
            var _loco = playerCore.LocomotionModule;

            Vector3 _dir = _loco.GetLastDirection();

            _loco.SetupJump(_dir);
            playerCore.AnimBoundary.JumpUpAnim();
        }

        public override void Update(PlayerCore playerCore)
        {
            var _loco = playerCore.LocomotionModule;
            var _ctrl = playerCore.CharacterCtrlBoudary;
            var _state = playerCore.StateMachineFlow;

            // ==================== Ground 체크
            // 실제 물리적 체크
            bool _isGroundDetected = _ctrl.CheckGround();
            // 물리체크이후 찐 Ground체크 (OnLeaveGround함수를 통해 무시(False로 유지)해야하는 경우도 발생하기에)
            bool _isGroundHit = _loco.UpdateGround(_isGroundDetected);

            // ==================== 연산
            // 중력
            _loco.ApplyGravity();
            // 최종 반영될 속도 
            Vector3 _finalVelocity = _loco.GetFinalVelocity();

            // ==================== 적용
            // 실제 이동
            _ctrl.SetMove(_finalVelocity);


            // ==================== 상태 전환
            if (_finalVelocity.y <= 0)
            {
                _state.ChangeLocoState(ELocomotionStateType.Fall);
            }
        }

        public override void Exit(PlayerCore playerCore)
        {

        }
    }
}
