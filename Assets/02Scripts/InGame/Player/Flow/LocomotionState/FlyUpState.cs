using UnityEngine;
using alpha.player.state;

namespace alpha.player.locomotion
{
    public class FlyUpState : PlayerStateBase
    {
        private float m_startTimer;
        private float m_startWaitingTime = 0.3f;
        private float m_nextTimer;
        private float m_nextWaitingTime = 0.2f; // 애니 길이에 맞춤
        public override void Enter(PlayerCore p_playerCore)
        {
            m_startTimer = 0;

            var loco = p_playerCore.LocomotionModule;
            var anim = p_playerCore.AnimBoundary;

            Vector3 dir = loco.GetLastDirection();

            loco.SetupFlyUp();
            anim.FlyUpAnim();
        }

        public override void Update(PlayerCore p_playerCore)
        {
            m_startTimer += Time.deltaTime;

            if (m_startTimer < m_startWaitingTime) return;

            var loco = p_playerCore.LocomotionModule;
            var ctrl = p_playerCore.CharacterCtrlBoudary;
            var state = p_playerCore.StateMachineFlow;

            // ==================== 연산
            bool isRising = loco.UpdateFlyUp();
            Vector3 finalVelocity = loco.GetFinalVelocity();
            // x,z 방향값 제거하여 수직 상승으로
            finalVelocity.x = 0;
            finalVelocity.z = 0;

            // ==================== 적용
            // 실제 이동
            ctrl.SetMove(finalVelocity);

            if (!isRising)
            {
                state.ChangeLocoState(ELocomotionStateType.Flight);
            }
        }

        public override void Exit(PlayerCore p_playerCore)
        {

        }
    }
}