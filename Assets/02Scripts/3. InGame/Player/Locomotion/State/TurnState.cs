using alpha.ingame.input;
using UnityEngine;
namespace alpha.ingame.player
{
    public class TurnState : LocomotionState
    {
        public TurnState(PlayerCore p_playerCore) : base(p_playerCore) { }

        public override void EnterState()
        {
            Debug.Log("Enter TurnState");
            // 현재 Animator의 Move 상태에 연결된 Turn을 트리거한다.

            EMoveType prev = _LocomotionModule.PrevMoveType;
            EMoveType current = _LocomotionModule.MoveType;

            _AnimationView.TurnAnim(prev, current);
        }
        public override void UpdateState()
        {
            if(_AnimationView.ShouldReturnFromTurn())
            {
                _StateMachine.ChangeState(ELocomotionState.Move);
            }
            
        }
        public override void ExitState(){ }
    }
}
