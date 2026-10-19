using alpha.ingame.input;
using UnityEngine;

namespace alpha.ingame.player
{
    public class IdleState : LocomotionState
    {
        public IdleState(PlayerCore p_playerCore) : base(p_playerCore){}

        public override void EnterState()
        {
            Debug.Log("Enter IdleState");
            _LocomotionModule.SetMoveType(Vector2.zero, false, false);
        }
        public override void UpdateState()
        {
            AlphaInput input = AlphaInput.Instance;
            Vector2 moveInput = input.MoveInput;
            bool isJump = input.IsJump;

            if (isJump)
            {
                if(input.IsJump && _LocomotionModule.CanGroundJump)
                {
                    _StateMachine.ChangeState(ELocomotionState.Jump);
                    return;
                }
            }

            if (moveInput.sqrMagnitude > 0.0001f)
            {
                _StateMachine.ChangeState(ELocomotionState.Move);
                return;
            }

            // 정지 중에도 감속과 중력을 계속 처리한다.
            _LocomotionModule.Move(Vector2.zero);
        }
        public override void ExitState(){}
    }
}
