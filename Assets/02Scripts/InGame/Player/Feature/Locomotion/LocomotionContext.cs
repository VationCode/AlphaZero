using UnityEngine;

namespace alpha.player.locomotion
{
    // Locomotion의 정보 저장 및 전달을 담당하는 클래스(Domain)
    public class LocomotionContext
    {
        public ELocomotionStateType CurrentStateType { get; set; } = ELocomotionStateType.Idle;
        public Vector3 CurrentMoveVelocity { get; set; } = Vector3.zero;
        public float CurrentMoveSpeed { get; set; } = 0f;

        public bool IsJumping { get; set; } = false;
        public bool IsDashing { get; set; } = false;

        public void Reset()
        {
            CurrentMoveVelocity = Vector3.zero;
            CurrentMoveSpeed = 0f;
            IsJumping = false;
            IsDashing = false;
        }

        public void SetCurrentStateType(ELocomotionStateType p_stateType)
        {
            CurrentStateType = p_stateType;
        }
        public void SetMoveVelocity(Vector3 p_velocity)
        {
            CurrentMoveVelocity = p_velocity;
        }
        public void SetMoveSpeed(float p_speed)
        {
            CurrentMoveSpeed = p_speed;
        }
        public void SetJumping(bool p_isJumping)
        {
            IsJumping = p_isJumping;
        }
        public void SetDashing(bool p_isDashing)
        {
            IsDashing = p_isDashing;
        }
    }
}
