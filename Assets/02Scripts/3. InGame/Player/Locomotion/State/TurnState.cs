using alpha.ingame.input;
using UnityEngine;
namespace alpha.ingame.player
{
    public class TurnState : LocomotionState
    {
        public TurnState(PlayerCore p_playerCore) : base(p_playerCore) { }

        public override void EnterState()
        {
            // 현재 Animator의 Move 상태에 연결된 Turn을 트리거한다.
            _AnimationView.TurnAnim();
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
