using UnityEngine;

namespace alpha.player.flow.locomotion
{
    public class FallState : PlayerStateBase
    {
        public override void Enter(PlayerCore playerCore)
        {
            playerCore.AnimBoundary.FallAnim();
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
            // 물리
            _ctrl.SetMove(_finalVelocity);

            // ==================== 상태 전환
            if (_isGroundHit)
            {
                _state.ChangeLocoState(LocomotionStateType.Land);
            }
        }

        public override void Exit(PlayerCore playerCore)
        {

        }
    }
}
