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
            bool isSprint = _InputSystem.IsSprint;
            
            Vector2 inputDir = _InputSystem.MoveInputDir;
            Vector3 velocity = _LocomotionModule.MoveVelocity(inputDir, isSprint);


            _LocomotionModule.Rotation(inputDir);

            _LocomotionModule.FinalMoveDirection(velocity);

            _AnimView.UpdateGroundMove(_InputSystem.MoveInputDir, isSprint, false);

            if (_InputSystem.MoveInputDir == Vector2.zero)
            {
                _LocomotionFlow.ChangeState(ELocomotionStateType.Idle);
            }
        }
        public override void Exit()
        {
            
        }
    }
}
