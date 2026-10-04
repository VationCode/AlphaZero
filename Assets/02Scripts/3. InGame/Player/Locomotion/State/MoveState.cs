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
            Vector2 moveInput = AlphaInput.Instance.MoveInput;
            bool isSprint = AlphaInput.Instance.IsSprint;
            bool isWalk = AlphaInput.Instance.IsWalk;
            bool isCombat = AlphaInput.Instance.IsCombat;

            // Turn 판정 전에 타겟 이동 타입을 저장한다.
            _LocomotionModule.SetMoveType(moveInput, isSprint, isWalk, isCombat);

            if(_LocomotionModule.IsOppositeDirection(moveInput))
            {
                _StateMachine.ChangeState(ELocomotionState.Turn);                
                return;
            }

            _LocomotionModule.Move(moveInput);

            if (moveInput.sqrMagnitude <= 0.0001f)
            {
                _StateMachine.ChangeState(ELocomotionState.Idle);
                return; // 한 프레임 입력이 없어도 이전 이동 타입은 유지
            }

            _LocomotionModule.SetPrevMoveType(_LocomotionModule.MoveType);
        }
        public override void ExitState(){}
    }
}
