using alpha.ingame.input;
using UnityEngine;

namespace alpha.ingame.player
{
    public class IdleState : LocomotionState
    {
        public IdleState(PlayerCore p_playerCore) : base(p_playerCore){}

        public override void EnterState()
        {
            
        }
        public override void UpdateState()
        {
            Vector2 moveInput = AlphaInput.Instance.MoveInput;

            // Idle에서는 Turn을 판정하지 않고 MoveState로 넘긴다.
            if (moveInput.sqrMagnitude > 0.0001f)
            {
                _StateMachine.ChangeState(ELocomotionState.Move);
                return;
            }

            _LocomotionModule.SetMoveType(Vector2.zero, AlphaInput.Instance.IsSprint, AlphaInput.Instance.IsWalk, AlphaInput.Instance.IsCombat);

            // 정지 중에도 감속과 중력을 계속 처리한다.
            _LocomotionModule.Move(Vector2.zero);
        }
        public override void ExitState()
        {
            
        }
    }
}
