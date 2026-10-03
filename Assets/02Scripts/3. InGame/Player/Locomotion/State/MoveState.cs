using alpha.ingame.input;
using UnityEngine;
namespace alpha.ingame.player
{
    public class MoveState : LocomotionState
    {
        public MoveState(PlayerCore p_playerCore) : base(p_playerCore){}

        public override void EnterState()
        {

        }
        public override void UpdateState()
        {
            Vector2 moveInput = AlphaInput.Instance.MoveInput;
            bool isSprint = AlphaInput.Instance.IsSprint;
            bool isWalk = AlphaInput.Instance.IsWalk;
            bool isCombat = AlphaInput.Instance.IsCombat;

            // Turn 판정 전에 현재 이동 타입을 저장한다.
            _LocomotionModule.SetMoveType(moveInput, isSprint, isWalk, isCombat);

            // 현재 Animator의 Move 상태에서만 Turn 전환을 요청한다.
            if (_LocomotionModule.MoveType != EMoveType.Combat &&
                _AnimationView.CanTurn() &&
                _LocomotionModule.IsOppositeDirection(moveInput))
            {
                _StateMachine.ChangeState(ELocomotionState.Turn);
                return;
            }

            _LocomotionModule.Move(moveInput);

            if (moveInput.sqrMagnitude <= 0.0001f)
                _StateMachine.ChangeState(ELocomotionState.Idle);
        }
        public override void ExitState()
        {
            
        }
    }
}
