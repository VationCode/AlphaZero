using UnityEngine;
using alpha.player.state;

namespace alpha.player.locomotion
{
    public class FlightState : PlayerStateBase
    {
        public override void Enter(PlayerCore p_playerCore)
        {
            var anim = p_playerCore.AnimBoundary;
            anim.FlightCrossFade();
        }
        public override void Update(PlayerCore p_playerCore)
        {
            var loco = p_playerCore.LocomotionModule;
            var anim = p_playerCore.AnimBoundary;
            var input = p_playerCore.InputSystemBoundary;
            var ctrl = p_playerCore.CharacterCtrlBoudary;
            var state = p_playerCore.StateMachineFlow;

            // ==================== 연산
            loco.HandleFlightMove(input.MoveInputDir);
            loco.HandleFlightRotation();
            Vector3 finalVelocity = loco.GetFinalVelocity();
            Vector3 horizontal = new Vector3(finalVelocity.x, 0, finalVelocity.z);

            // ==================== 적용
            // 실제 이동
            ctrl.SetMove(finalVelocity);
            anim.FlightAnim(horizontal);

            if(input.IsFlyInput)
            {
                state.ChangeLocoState(ELocomotionStateType.Fall);
            }
        }

        public override void Exit(PlayerCore p_playerCore)
        {

        }
    }
}
