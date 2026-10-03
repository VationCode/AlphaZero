using alpha.ingame.input;
using UnityEngine;
namespace alpha.ingame.player
{
    public class TurnState : LocomotionState
    {
        public TurnState(PlayerCore p_playerCore) : base(p_playerCore) { }

        public override void EnterState()
        {
            // LocomotionModule에 저장된 이동 타입으로 클립을 선택한다.
            _AnimationView.TurnAnim(_LocomotionModule.MoveType);
        }
        public override void UpdateState()
        {
            // Turn 중에는 일반 Move()와 Rotation()을 호출하지 않는다.
            if (!_AnimationView.IsTurnFinished())
                return;

            bool hasInput = AlphaInput.Instance.MoveInput.sqrMagnitude > 0.0001f;

            _StateMachine.ChangeState(hasInput ? ELocomotionState.Move : ELocomotionState.Idle);
        }
        public override void ExitState()
        {
            _AnimationView.SetTurnRootMotion(false);
        }
    }
}
