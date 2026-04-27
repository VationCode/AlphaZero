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
            playerCore.LocomotionModule.Movement();
            playerCore.LocomotionModule.LocomotionAni();
        }

        public override void Exit(PlayerCore playerCore)
        {

        }

    }
}