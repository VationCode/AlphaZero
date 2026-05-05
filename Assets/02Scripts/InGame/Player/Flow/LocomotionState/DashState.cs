using UnityEngine;
namespace alpha.player.flow.locomotion
{
    public class DashState : PlayerStateBase
    {
        private float m_timer;
        private float m_waitingTime = 0.2f; // 애니 길이에 맞춤
        public override void Enter(PlayerCore playerCore)
        {
            m_timer = 0;

            var _loco = playerCore.LocomotionModule;
            var _anim = playerCore.AnimBoundary;

            Vector3 _dir = _loco.GetLastDirection();
            _loco.SetupDash(_dir);

            _anim.DashAnim();
        }

        public override void Update(PlayerCore playerCore)
        {
            var _loco = playerCore.LocomotionModule;
            var _ctrl = playerCore.CharacterCtrlBoudary;
            var _state = playerCore.StateMachineFlow;

            // ==================== 연산
            // 지정 방향으로의 프레임당 이동값
            bool _isDashing = _loco.UpdateDash();
            
            // 최종 반영될 속도 
            Vector3 _finalVelocity = _loco.GetFinalVelocity();

            // ==================== 적용
            // 물리
            _ctrl.SetMove(_finalVelocity);

            if (!_isDashing)
            {
                m_timer += Time.deltaTime;
                if (m_waitingTime > m_timer)
                {
                    playerCore.StateMachineFlow.ChangeLocoState(LocomotionStateType.Move);
                }
            }

        }

        public override void Exit(PlayerCore playerCore)
        {

        }
    }
}
