using alpha.ingame.input;
using UnityEngine;
namespace alpha.ingame.player
{
    public class MobilityState : LocomotionState
    {
        private EAreteType _areteType;
        private float _leapElapsed;

        public MobilityState(PlayerCore p_playerCore) : base(p_playerCore) { }

        public override void EnterState()
        {
            _areteType = _PlayerCore.CurrentAreteType;
            _leapElapsed = 0f;

            if (_areteType == EAreteType.Heruna)
            {
                _LocomotionModule.BeginLeap(AlphaInput.Instance.MoveInput);
                _LocomotionModule.LeapMove();
                _leapElapsed = Time.deltaTime;
            }
            else if (_areteType == EAreteType.Silyon)
            {
                AlphaInput input = AlphaInput.Instance;
                _LocomotionModule.SetBoostMoveType(IsCombatStanceActive);
                _LocomotionModule.Move(input.MoveInput);
            }
        }

        public override void UpdateState()
        {
            AlphaInput input = AlphaInput.Instance;
            if (_areteType == EAreteType.Heruna)
                UpdateLeap(input);
            else if (_areteType == EAreteType.Silyon)
                UpdateBoost(input);
        }

        public override void ExitState()
        {
            AlphaInput input = AlphaInput.Instance;
            _LocomotionModule.SetMoveType(input.MoveInput, input.IsWalk, IsCombatStanceActive);
        }

        private void UpdateLeap(AlphaInput input)
        {
            if (input.IsJump && _LocomotionModule.CanAirJump)
            {
                _StateMachine.ChangeState(ELocomotionState.Jump);
                return;
            }

            _leapElapsed += Time.deltaTime;
            if (_leapElapsed <= _LocomotionModule.LeapDuration)
                _LocomotionModule.LeapMove();
            else
                _LocomotionModule.AirMove(input.MoveInput);

            if (_LocomotionModule.IsGrounded && _LocomotionModule.VelocityY <= 0f)
                _StateMachine.ChangeState(ELocomotionState.Land);
            else if (_LocomotionModule.VelocityY <= 0f)
                _StateMachine.ChangeState(ELocomotionState.Fall);
        }

        private void UpdateBoost(AlphaInput input)
        {
            if (!input.IsMobilityHeld)
            {
                ReturnToMovement(input);
                return;
            }

            if (input.IsJump && _LocomotionModule.CanGroundJump)
            {
                _StateMachine.ChangeState(ELocomotionState.Jump);
                return;
            }

            _LocomotionModule.SetBoostMoveType(IsCombatStanceActive);
            _LocomotionModule.Move(input.MoveInput);
            if (_LocomotionModule.IsFalling)
                _StateMachine.ChangeState(ELocomotionState.Fall);
        }

        private void ReturnToMovement(AlphaInput input)
        {
            if (!_LocomotionModule.IsGrounded)
            {
                _StateMachine.ChangeState(ELocomotionState.Fall);
                return;
            }

            ELocomotionState next = input.MoveInput.sqrMagnitude > 0.0001f || IsCombatStanceActive
                ? ELocomotionState.Move
                : ELocomotionState.Idle;
            _StateMachine.ChangeState(next);
        }
    }
}
