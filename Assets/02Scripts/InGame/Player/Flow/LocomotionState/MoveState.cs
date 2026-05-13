using UnityEngine;
using alpha.player.state;


namespace alpha.player.locomotion
{
    public class MoveState : PlayerStateBase
    {
        public override void Enter(PlayerCore p_playerCore)
        {
            
        }
        public override void Update(PlayerCore p_playerCore)
        {
            var loco = p_playerCore.LocomotionModule;
            var input = p_playerCore.InputSystemBoundary;
            var ctrl = p_playerCore.CharacterCtrlBoudary;
            var anim = p_playerCore.AnimBoundary;
            var state = p_playerCore.StateMachineFlow;

            // ==================== Ground 체크
            // 실제 물리적 체크
            bool isGroundDetected = ctrl.CheckGround();
            // 물리체크이후 찐 Ground체크 (OnLeaveGround함수를 통해 무시(False로 유지)해야하는 경우도 발생하기에)
            bool isGroundHit = loco.UpdateGround(isGroundDetected);

            // ==================== 연산
            // 중력
            loco.ApplyGravity();
            // 이동
            loco.HandleMove(false, input.MoveInputDir);
            // 회전
            loco.HandleRotation(false);
            // 최종 반영될 속도 
            Vector3 finalVelocity = loco.GetFinalVelocity();
            Vector3 horizontal = new Vector3(finalVelocity.x, 0, finalVelocity.z);   //Ground이동 애니메이션이기에 y제거
            
            // ==================== 적용
            // 실제 이동
            ctrl.SetMove(finalVelocity);
            // 애니메이션
            anim.UpdateGroundMove(horizontal);

            // ==================== 상태 전환
            if (input.IsJumpInput)
            {
                state.ChangeLocoState(ELocomotionStateType.JumpUp);
            }
            else if (input.IsDashInput)
            {
                state.ChangeLocoState(ELocomotionStateType.Dash);
            }
            else if (!isGroundHit)
            {
                state.ChangeLocoState(ELocomotionStateType.Fall);
            }
            else if (input.IsFlyInput)
            {
                state.ChangeLocoState(ELocomotionStateType.FlyUp);
            }
        }

        public override void Exit(PlayerCore p_playerCore)
        {
            p_playerCore.AnimBoundary.UpdateGroundMove(Vector3.zero);
        }

    }
}