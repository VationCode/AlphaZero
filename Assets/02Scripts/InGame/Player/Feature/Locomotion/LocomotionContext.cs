using UnityEngine;

namespace alpha.player.locomotion
{
    // Locomotion의 정보 저장 및 전달을 담당하는 클래스(Domain)
    public class LocomotionContext
    {
        public Vector3 LastMoveDirection { get; private set; }
        public Vector2 LastMoveInput { get; private set; }

        public Vector3 HorizontalVelocity { get; private set; }
        public float VerticalVelocity { get; private set; }

        public bool IsGrounded { get; private set; }

        public bool IsJumping { get; private set; }
        public bool IsDashing { get; private set; }

        public Vector2 DodgeInput { get; private set; }

        public void Reset()
        {
            LastMoveDirection = Vector3.zero;
            LastMoveInput = Vector2.zero;

            HorizontalVelocity = Vector3.zero;
            VerticalVelocity = 0f;

            DodgeInput = Vector2.zero;

            IsGrounded = false;
            IsJumping = false;
            IsDashing = false;
        }

        public void SetLastMoveDirection(Vector3 p_dir)
        {
            LastMoveDirection = p_dir;
        }
        public void SetLastMoveInput(Vector2 p_input)
        {
            if (p_input.sqrMagnitude < 0.01f)
                return;

            LastMoveInput = p_input;
        }


        public void SetHorizontalVelocity(Vector3 p_velocity)
        {
            HorizontalVelocity = p_velocity;
        }
        
        public void SetVerticalVelocity(float p_velocityY)
        {
            VerticalVelocity = p_velocityY;
        }

        public void SetGrounded(bool p_isGrounded)
        {
            IsGrounded = p_isGrounded;
        }

        public void SetJumping(bool p_isJumping)
        {
            IsJumping = p_isJumping;
        }

        public void SetDashing(bool p_isDashing)
        {
            IsDashing = p_isDashing;
        }
        public void SetDodgeInput(Vector2 p_input)
        {
            DodgeInput = p_input;
        }
        public void ClearDodgeInput()
        {
            DodgeInput = Vector2.zero;
        }
    }
}
