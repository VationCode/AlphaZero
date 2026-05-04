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
            // ==================== Ground 체크
            // 실제 물리적 체크
            bool _isGroundDetected = playerCore.CharacterCtrlBoudary.CheckGround();
            // 물리체크이후 찐 Ground체크 (OnLeaveGround함수를 통해 무시(False로 유지)해야하는 경우도 발생하기에)
            bool _isGroundHit = playerCore.LocomotionModule.UpdateGround(_isGroundDetected);

            // ==================== 연산
            // 중력
            playerCore.LocomotionModule.ApplyGravity();
            // 최종 반영될 속도 
            Vector3 _finalVelocity = playerCore.LocomotionModule.GetFinalVelocity();

            // ==================== 적용
            // 물리
            playerCore.CharacterCtrlBoudary.SetMove(_finalVelocity);

            // ==================== 상태 전환
            if (_isGroundHit)
            {
                playerCore.StateMachineFlow.ChangeLocoState(LocomotionStateType.Land);
            }
        }

        public override void Exit(PlayerCore playerCore)
        {

        }
    }
}
