using UnityEngine;

namespace alpha.player.locomotion
{
    public class IdleState : LocomotionState
    {
        public IdleState(PlayerCore core) : base(core){}

        public override void Enter()
        {
            base.Enter();
            // 이동에 관한 설정들 초기화

            // 애니메이션 동작
            _AnimView.UpdateGroundMove(Vector3.zero);
        }
        public override void Update()
        {
            // 키 입력에 따른 상태 전환
            if(_InputSystem.IsDodge)
            {
                _LocomotionFlow.ChangeState(ELocomotionStateType.Dodge);
                return;
            }

            if (_InputSystem.MoveInputDir != Vector2.zero)
            {
                _LocomotionFlow.ChangeState(ELocomotionStateType.Move);
            }
        }

        public override void Exit()
        {
            
        }
    }
}