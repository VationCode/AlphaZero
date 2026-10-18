using alpha.ingame.input;
using UnityEngine;
namespace alpha.ingame.player
{
    public class LandState : LocomotionState
    {
        public LandState(PlayerCore p_playerCore) : base(p_playerCore) { }

        public override void EnterState()
        {
            _LocomotionModule.ResetJumpCount();
            _AnimationView.LandAnim();
        }
        public override void UpdateState()
        {
            /*_LocomotionModule.HoldGround(); // 수평 이동 없이 접지 유지

            if (!_AnimationView.IsLandAnimFinished())
                return;*/

            _StateMachine.ChangeState(ELocomotionState.Idle);
        }
        public override void ExitState()
        {

        }
    }
}
