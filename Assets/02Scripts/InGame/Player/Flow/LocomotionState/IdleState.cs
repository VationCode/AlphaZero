using UnityEngine;
using alpha.player.state;

namespace alpha.player.locomotion
{
    public class IdleState : PlayerStateBase
    {
        public override void Enter(PlayerCore p_playerCore)
        {
            Debug.Log("IdleState");
        }
        public override void Update(PlayerCore p_playerCore)
        {
            if (p_playerCore.InputSystemBoundary.MoveInputDir != Vector2.zero)
                p_playerCore.StateMachineFlow.ChangeLocoState(ELocomotionStateType.Move);
        }

        public override void Exit(PlayerCore p_playerCore)
        {
           
        }


    }
}
