using alpha.ingame.input;
using UnityEngine;

namespace alpha.ingame.player
{
    /// <summary>헤르나의 짧은 도약과 도약 후 공중 이동을 관리한다.</summary>
    public sealed class HernaArete : Arete
    {
        private float _leapElapsed;

        public override EAreteType AreteType => EAreteType.Heruna;
        public override bool CanUseMobility => true;

        public override void BeginMobility(AlphaInput input, bool isCombatStance)
        {
            Locomotion.BeginLeap(input.MoveInput);
            Locomotion.LeapMove();
            _leapElapsed = Time.deltaTime;
        }

        public override ELocomotionState? UpdateMobility(AlphaInput input, bool isCombatStance)
        {
            if (input.IsJump && Locomotion.CanAirJump)
                return ELocomotionState.Jump;

            _leapElapsed += Time.deltaTime;
            if (_leapElapsed <= Locomotion.LeapDuration)
                Locomotion.LeapMove();
            else
                Locomotion.AirMove(input.MoveInput);

            if (Locomotion.IsGrounded && Locomotion.VelocityY <= 0f)
                return ELocomotionState.Land;
            if (Locomotion.VelocityY <= 0f)
                return ELocomotionState.Fall;
            return null;
        }

        public override void EndMobility() { }
    }
}
