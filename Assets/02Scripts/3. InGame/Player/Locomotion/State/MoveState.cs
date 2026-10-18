using alpha.ingame.input;
using UnityEngine;
namespace alpha.ingame.player
{
    public class MoveState : LocomotionState
    {
        public MoveState(PlayerCore p_playerCore) : base(p_playerCore){}

        public override void EnterState()
        {
            Debug.Log("Enter MoveState");
        }
        public override void UpdateState()
        {
            AlphaInput input = AlphaInput.Instance;
            Vector2 moveInput = input.MoveInput;
            bool isWalk = input.IsWalk;
            bool isCombat = input.IsCombat;
            bool isJump = input.IsJump;

            _LocomotionModule.SetMoveType(moveInput, isWalk, isCombat);

            if (isJump)
            {
                if (input.IsJump && _LocomotionModule.CanGroundJump)
                {
                    _StateMachine.ChangeState(ELocomotionState.Jump);
                    return;
                }
            }

            _LocomotionModule.Move(moveInput);

            if (moveInput.sqrMagnitude <= 0.0001f)
            {
                _StateMachine.ChangeState(ELocomotionState.Idle);
                return; // 한 프레임 입력이 없어도 이전 이동 타입은 유지
            }

        }
        public override void ExitState(){}
    }
}
