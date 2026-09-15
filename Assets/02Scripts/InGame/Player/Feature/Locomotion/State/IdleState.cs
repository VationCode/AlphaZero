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
            // 중력 적용
            Vector3 velocity = Vector3.zero;
            velocity.y = _Core.LocomotionContext.VerticalVelocity;
            _LocomotionModule.FinalMoveDirection(velocity);

            
            if (_Input.IsDodge)
            {
                    _Core.LocomotionContext.SetDodgeInput(_Input.MoveInputDir);

                    _LocomotionFlow.ChangeState(ELocomotionStateType.Dodge);
                return;
            }

            if (_Input.IsDash)
            {
                _LocomotionFlow.ChangeState(ELocomotionStateType.Dash);
                return;
            }

            if (_Input.MoveInputDir != Vector2.zero)
            {
                _LocomotionFlow.ChangeState(ELocomotionStateType.Move);
            }
        }

        public override void Exit()
        {
            
        }
    }
}