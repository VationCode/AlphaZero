using UnityEngine;
using alpha.player.state;

namespace alpha.player.locomotion
{
    public class JumpUpState : PlayerStateBase
    {
        public override EBlockedCombatAction BlockedCombatAction => EBlockedCombatAction.Swap | EBlockedCombatAction.InCombat | EBlockedCombatAction.Attack | EBlockedCombatAction.Skill;

        public override void Enter(PlayerCore p_playerCore)
        {
            var loco = p_playerCore.LocomotionModule;

            Vector3 dir = loco.GetLastDirection();

            loco.SetupJump(dir);
            p_playerCore.AnimBoundary.JumpUpAnim();
        }

        public override void Update(PlayerCore p_playerCore)
        {
            var loco = p_playerCore.LocomotionModule;
            var ctrl = p_playerCore.CharacterCtrlBoudary;
            var state = p_playerCore.StateMachineFlow;

            // ==================== Ground 체크
            // 실제 물리적 체크
            bool isGroundDetected = ctrl.CheckGround();
            // 물리체크이후 찐 Ground체크 (OnLeaveGround함수를 통해 무시(False로 유지)해야하는 경우도 발생하기에)
            bool isGroundHit = loco.UpdateGround(isGroundDetected);

            // ==================== 연산
            // 중력
            loco.ApplyGravity();
            // 최종 반영될 속도 
            Vector3 finalVelocity = loco.GetFinalVelocity();

            // ==================== 적용
            // 실제 이동
            ctrl.SetMove(finalVelocity);


            // ==================== 상태 전환
            if (finalVelocity.y <= 0)
            {
                state.ChangeLocoState(ELocomotionStateType.Fall);
            }
        }

        public override void Exit(PlayerCore p_playerCore)
        {

        }
    }
}
