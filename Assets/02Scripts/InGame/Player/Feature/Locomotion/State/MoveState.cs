using UnityEngine;

namespace alpha.player.locomotion
{
    public class MoveState : LocomotionState
    {
        public MoveState(PlayerCore core) : base(core){}

        public override void Enter()
        {
            base.Enter();
        }
        public override void Update()
        {
            bool isSprint = _Input.IsSprint;
            
            Vector2 inputDir = _Input.MoveInputDir;
            Vector3 velocity = _LocomotionModule.MoveVelocity(inputDir, isSprint);
            
            if(_Input.IsDodge)
            {
                _Core.LocomotionContext.SetDodgeInput(inputDir);

                _LocomotionFlow.ChangeState(ELocomotionStateType.Dodge);
                return;
            }

            _LocomotionModule.Rotation(inputDir);

            Vector3 horizontalVelocity = velocity;

            velocity.y = _Core.LocomotionContext.VerticalVelocity;

            _LocomotionModule.FinalMoveDirection(velocity);

            Vector2 localMoveDir = _LocomotionModule.GetLocalDirection(horizontalVelocity);

            _AnimView.UpdateGroundMove(localMoveDir, isSprint, false);

            if(_Input.IsDash)
            {
                _LocomotionFlow.ChangeState(ELocomotionStateType.Dash);
                return;
            }

            if (_Input.MoveInputDir == Vector2.zero)
            {
                _LocomotionFlow.ChangeState(ELocomotionStateType.Idle);
            }
        }
        public override void Exit()
        {
            
        }

        private Vector2 GetDodgeInput(Vector2 p_currentInput)
        {
            if (p_currentInput.sqrMagnitude >= 0.01f)
                return p_currentInput;

            // 입력값이 없을 경우 마지막 입력값으로
            Vector2 lastMoveInput = _Core.LocomotionContext.LastMoveInput;

            if (lastMoveInput.sqrMagnitude >= 0.01f)
                return lastMoveInput;
            // 마지막 입력값도 없을경우 forward방향으로
            return Vector2.up;
        }
    }
}
