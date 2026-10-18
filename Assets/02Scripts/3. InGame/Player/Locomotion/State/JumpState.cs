using alpha.ingame.input;
using UnityEngine;
namespace alpha.ingame.player
{
    public class JumpState : LocomotionState
    {
        public JumpState(PlayerCore p_playerCore) : base(p_playerCore){}

        public override void EnterState()
        {
            Debug.Log("Enter Jump");
            _LocomotionModule.Jump();
            _LocomotionModule.AirMove(AlphaInput.Instance.MoveInput);
        }
        public override void UpdateState()
        {
            AlphaInput input = AlphaInput.Instance;
            Vector2 moveInput = input.MoveInput;

            // 상승 중 2단 점프는 같은 Jump 상태에서 바로 실행한다.
            if (input.IsJump && _LocomotionModule.CanAirJump)
                _LocomotionModule.Jump();

            _LocomotionModule.AirMove(moveInput); // 이동 후 수직 속도와 접지 상태 확인

            if (_LocomotionModule.IsGrounded)
            {
                // 정점 전에 접지한 경우의 처리. 착지 상태 구현 시 옮길 부분.
                _LocomotionModule.ResetJumpCount();
                _StateMachine.ChangeState(moveInput.sqrMagnitude > 0.0001f? ELocomotionState.Move : ELocomotionState.Idle);
                return;
            }

            if (_LocomotionModule.VelocityY <= 0f)
                _StateMachine.ChangeState(ELocomotionState.Fall);
        }
        public override void ExitState()
        {

        }
    }
}