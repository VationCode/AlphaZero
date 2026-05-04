using UnityEngine;

namespace alpha.player.flow.locomotion
{
    public class LandState : PlayerStateBase
    {
        private float m_timer;
        private float m_duration = 0.2f; // 애니 길이에 맞춤

        public override void Enter(PlayerCore playerCore)
        {
            m_timer = 0f;
            playerCore.AnimBoundary.LandAnim();
        }

        public override void Update(PlayerCore playerCore)
        {
            m_timer += Time.deltaTime;

            if (m_timer >= m_duration)
            {
                playerCore.StateMachineFlow.ChangeLocoState(LocomotionStateType.Move);
            }
        }

        public override void Exit(PlayerCore playerCore)
        {

        }
    }
}