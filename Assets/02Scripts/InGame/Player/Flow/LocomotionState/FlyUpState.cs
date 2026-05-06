using UnityEngine;
using static Unity.Collections.AllocatorManager;
namespace alpha.player.flow.locomotion
{
    public class FlyUpState : PlayerStateBase
    {
        private float m_startTimer;
        private float m_startWaitingTime = 0.3f;
        private float m_nextTimer;
        private float m_nextWaitingTime = 0.2f; // 애니 길이에 맞춤
        public override void Enter(PlayerCore playerCore)
        {
            m_startTimer = 0;

            var _loco = playerCore.LocomotionModule;
            var _anim = playerCore.AnimBoundary;

            Vector3 _dir = _loco.GetLastDirection();

            _loco.SetupFlyUp();
            _anim.FlyUpAnim();
        }

        public override void Update(PlayerCore playerCore)
        {
            m_startTimer += Time.deltaTime;

            if (m_startTimer < m_startWaitingTime) return;

            var _loco = playerCore.LocomotionModule;
            var _ctrl = playerCore.CharacterCtrlBoudary;
            var _state = playerCore.StateMachineFlow;

            // ==================== 연산
            bool isRising = _loco.UpdateFlyUp();
            Vector3 _finalVelocity = _loco.GetFinalVelocity();
            // x,z 방향값 제거하여 수직 상승으로
            _finalVelocity.x = 0;
            _finalVelocity.z = 0;

            // ==================== 적용
            // 실제 이동
            _ctrl.SetMove(_finalVelocity);

            if (!isRising)
            {
                _state.ChangeLocoState(LocomotionStateType.Flight);
            }
        }

        public override void Exit(PlayerCore playerCore)
        {

        }
    }
}