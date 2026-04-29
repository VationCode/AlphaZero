using UnityEngine;


namespace alpha.player.flow.locomotion
{
    public class MoveState : PlayerStateBase
    {
        public override void Enter(PlayerCore playerCore)
        {

        }
        public override void Update(PlayerCore playerCore)
        {
            var _loco = playerCore.LocomotionModule;
            var _input = playerCore.InputSystemBoundary;

            playerCore.LocomotionModule.Movement();
            playerCore.LocomotionModule.LocomotionAni();

            if (_input.IsJumpInput)
            {
                playerCore.StateMachineFlow.ChangeLocoState(LocomotionStateType.JumpUp);
            }
            else if (_input.IsDashInput)
            {
                playerCore.StateMachineFlow.ChangeLocoState(LocomotionStateType.DashStart);
            }
        }

        public override void Exit(PlayerCore playerCore)
        {

        }

    }
}