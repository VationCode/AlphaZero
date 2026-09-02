using UnityEngine;

namespace alpha.player.locomotion
{
    // Locomotion의 정보 저장 및 전달을 담당하는 클래스(Domain)
    public class LocomotionContext
    {
        public ELocomotionMode CurrentLocomotionMode { get; set; } = ELocomotionMode.Ground;
        public Vector3 CurrentMoveDirection { get; set; } = Vector3.zero;
        public float CurrentMoveSpeed { get; set; } = 0f;

        public bool IsJumping { get; set; } = false;
        public bool IsDashing { get; set; } = false;

        public void Reset()
        {
            CurrentLocomotionMode = ELocomotionMode.Ground;
            CurrentMoveDirection = Vector3.zero;
            CurrentMoveSpeed = 0f;
            IsJumping = false;
            IsDashing = false;
        }

        public void SetLocomotionMode(ELocomotionMode mode)
        {
            CurrentLocomotionMode = mode;
        }
        public void SetMoveDirection(Vector3 direction)
        {
            CurrentMoveDirection = direction;
        }
        public void SetMoveSpeed(float speed)
        {
            CurrentMoveSpeed = speed;
        }
        public void SetJumping(bool isJumping)
        {
            IsJumping = isJumping;
        }
        public void SetDashing(bool isDashing)
        {
            IsDashing = isDashing;
        }
    }
}
