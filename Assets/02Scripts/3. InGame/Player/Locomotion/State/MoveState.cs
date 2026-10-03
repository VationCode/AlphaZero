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

            // Combat Turn 클립은 현재 구성에 없으므로 제외한다.
            if (_AnimationView.CanTurn(_LocomotionModule.MoveType) &&
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
