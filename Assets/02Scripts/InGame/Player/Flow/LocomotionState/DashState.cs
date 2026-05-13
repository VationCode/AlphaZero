using UnityEngine;
using alpha.player.state;

namespace alpha.player.locomotion
{
    public class DashState : PlayerStateBase
    {
        private float m_timer;
        private float m_waitingTime = 0.2f; // 애니 길이에 맞춤
        public override void Enter(PlayerCore p_playerCore)
        {
            m_timer = 0;

            var loco = p_playerCore.LocomotionModule;
            var anim = p_playerCore.AnimBoundary;

            Vector3 dir = loco.GetLastDirection();
            loco.SetupDash(dir);

            anim.DashAnim();
        }

        public override void Update(PlayerCore p_playerCore)
        {
            var loco = p_playerCore.LocomotionModule;
            var ctrl = p_playerCore.CharacterCtrlBoudary;
            var state = p_playerCore.StateMachineFlow;

            // ==================== 연산
            // 지정 방향으로의 프레임당 이동값
            bool isDashing = loco.UpdateDash();
            
            // 최종 반영될 속도 
            Vector3 finalVelocity = loco.GetFinalVelocity();

            // ==================== 적용
            // 실제 이동
            ctrl.SetMove(finalVelocity);

            if (!isDashing)
            {
                m_timer += Time.deltaTime;
                if (m_waitingTime > m_timer)
                {
                    p_playerCore.StateMachineFlow.ChangeLocoState(ELocomotionStateType.Move);
                }
            }

        }

        public override void Exit(PlayerCore p_playerCore)
        {

        }
    }
}
