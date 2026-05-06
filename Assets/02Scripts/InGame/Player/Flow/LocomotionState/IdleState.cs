using UnityEngine;
using alpha.player.state;

namespace alpha.player.locomotion
{
    public class IdleState : PlayerStateBase
    {
        public override void Enter(PlayerCore playerCore)
        {
            Debug.Log("IdleState");
        }
        public override void Update(PlayerCore playerCore)
        {
            if (playerCore.InputSystemBoundary.MoveInputDir != Vector2.zero)
                playerCore.StateMachineFlow.ChangeLocoState(ELocomotionStateType.Move);
        }

        public override void Exit(PlayerCore playerCore)
        {
           
        }


    }
}
