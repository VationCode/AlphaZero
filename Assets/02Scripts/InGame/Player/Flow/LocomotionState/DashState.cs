using UnityEngine;
namespace alpha.player.flow.locomotion
{
    public class DashState : PlayerStateBase
    {
        private float m_timer;
        private float m_waitingTime = 0.5f; // 애니 길이에 맞춤
        public override void Enter(PlayerCore playerCore)
        {
            var _loco = playerCore.LocomotionModule;

            Vector3 _dir = _loco.GetLastDirection();
            _loco.DashStart(_dir);

            playerCore.AnimBoundary.DashAnim();
        }

        public override void Update(PlayerCore playerCore)
        {
            m_timer += Time.deltaTime;

            if (m_timer >= m_waitingTime)
            {
                playerCore.StateMachineFlow.ChangeLocoState(LocomotionStateType.Move);
            }
        }

        public override void Exit(PlayerCore playerCore)
        {

        }
    }
}
