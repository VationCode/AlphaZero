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

            if (_StateMachine.TryEnterMobility())
                return;

            _LocomotionModule.Move(moveInput);

            // 지상 이동 중 턱을 벗어나면 입력 상태보다 낙하를 우선한다.
            if (_LocomotionModule.IsFalling)
            {
                _StateMachine.ChangeState(ELocomotionState.Fall);
                return;
            }

            // 전투 입력 중에는 방향 입력이 없어도 CombatMove Tree의 대기를 유지한다.
            if (moveInput.sqrMagnitude <= 0.0001f && !isCombat)
            {
                _StateMachine.ChangeState(ELocomotionState.Idle);
                return;
            }
        }
        public override void ExitState(){}
    }
}
