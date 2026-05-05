using UnityEngine;
namespace alpha.player.flow.locomotion
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
                playerCore.StateMachineFlow.ChangeLocoState(LocomotionStateType.Move);
        }

        public override void Exit(PlayerCore playerCore)
        {
           
        }


    }
}
