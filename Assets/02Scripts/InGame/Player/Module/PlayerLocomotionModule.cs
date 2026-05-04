// Module : 기능 연산 위주
using alpha.player.boundary;
using UnityEngine;

namespace alpha.player.module
{
    // 플레이어의 이동과 관련된 기능들을 담당하는 모듈
    public class PlayerLocomotionModule : MonoBehaviour
    {
        #region Config 
        [Header("[ Move ]")]
        [SerializeField] 
        private float m_baseMoveSpeed = 8;
        // [SerializeField] private float m_backMoveSpeed; 현재는 단순화로 진행하여 이후 애니메이션 늘리기
        [SerializeField] 
        private float m_combatMoveSpeed = 5;
        private float m_moveAnismoothTime = 0.1f;

        [Header("[ rotation ]")]
        [SerializeField] private float m_rotationsmoothTime = 0.1f;

        [Header("[ Jump ]")]
        [SerializeField]
        private float m_jumpPower = 3f;
        // 점프 시 수평 이동 속도 보정값 (0.1~1 사이)
        [SerializeField, Range(0.1f, 1)]
        private float m_jumpDirshorten = 0.5f;
        private float m_jumpIgnoreGroundTime = 0.2f;

        private enum AirState
        {
            None,
            Jump,
            Fall,
            Land,
            Dash,
            DashEnd
        }
        private AirState m_airState;

        [Header("[ Fall ]")]
        [SerializeField]
        private float m_fallMultiplier = 2f;    // 중력 가속도 보정값 (1보다 커야 빠르게 떨어짐)

        [Header("[ Land ]")]
        [SerializeField]
        private float m_landDuration = 0.2f;

        [Header("[ Dash ]")]
        // 대쉬는 속도보단 시간과 거리에 초점이 맞춰지는게 좋음
        [SerializeField]
        private float m_dashDistance = 10;
        [SerializeField]
        private float m_dashDuration = 0.5f;
        [SerializeField]
        private float m_dashEndDuration = 0.2f;

        [Header("[ Fly ]")]
        [SerializeField]
        private float m_flyUpPower;

        [Header("[ Gravity ]")]
        [SerializeField] private float m_gravityPower = -9.8f;
        #endregion

        #region Runtime
        // Move
        private float m_currentMoveSpeed;
        private Vector3 m_currentDir;
        private Vector3 m_currentVelocityXZ;
        private float m_currentVelocityY;

        private float m_moveAniMagnitude;
        private float m_moveAniVelocity;
        private Vector3 m_lastMoveDir;

        // Ground
        private bool m_isGrounded;
        private float m_lastGroundTime;
        private float m_lastLeaveGroundTime;
        private float m_groundIgnoreDuration;

        //Rotation
        private float m_rotationSmoothVelocity;


        // Jump
        private bool m_isJumping;
        private float m_lastJumpTime;
        private AirState m_prevAirState;

        // Fall
        // Land
        private float m_landTimer;

        // Dash
        private bool m_isDashing;
        private float m_dashLastTimer;
        private float m_dashEndTimer;
        #endregion

        private void Start()
        {
            m_airState = AirState.None;
            m_isGrounded = true;
        }

        public Vector3 GetFinalVelocity()
        {
            return m_currentVelocityXZ + Vector3.up * m_currentVelocityY;
        }
        public void SetHorizontalVelocity(Vector3 velocity)
        {
            m_currentVelocityXZ = velocity;
        }
        public Vector3 GetLastDirection()
        {
            return m_currentDir;
        }

        public void HandleMove(bool isCombat, Vector2 moveDir)
        {
            // 카메라기준으로 캐릭터 이동
            Vector3 _forward = Camera.main.transform.forward;
            _forward.y = 0f;    // y값에 따라 높이가 변해버리기에 0으로 설정
            Vector3 _right = Camera.main.transform.right;

            // 방향 계산
            Vector2 _moveInputDir = moveDir;
            Vector3 _Dir = _forward * _moveInputDir.y + _right * _moveInputDir.x;

            // 값 보정
            if (_Dir.sqrMagnitude < 0.01f) _Dir = Vector2.zero;
            else _Dir = Vector3.ClampMagnitude(_Dir, 1f);

            // 속력 계산
            float _speed = isCombat ? m_combatMoveSpeed : m_baseMoveSpeed;

            // 속도 계산
            Vector3 _velocity = _Dir * _speed;

            m_currentDir = _Dir;
            m_currentMoveSpeed = _speed;
            m_currentVelocityXZ = _velocity;
        }

        public void HandleRotation(bool instant = false)
        {
            if (m_currentDir == Vector3.zero) return;
            if (m_isDashing) return;

            Quaternion targetRot = Quaternion.LookRotation(m_currentDir);

            if (instant)
            {
                transform.rotation = targetRot;
                return;
            }

            float targetAngle = targetRot.eulerAngles.y;

            float smoothedAngle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref m_rotationSmoothVelocity,
                m_rotationsmoothTime
            );

            transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
        }

        // 잠시 Ground체크 무시하는 시간
        public void OnLeaveGround(float ignoreTime)
        {
            m_lastLeaveGroundTime = Time.time;
            m_groundIgnoreDuration = ignoreTime;
        }

        public bool UpdateGround(bool isGroundDetected)
        {
            if ((Time.time - m_lastLeaveGroundTime) < m_groundIgnoreDuration)
            {
                m_isGrounded = false;
                return m_isGrounded;
            }

            if (isGroundDetected)
            {
                m_lastGroundTime = Time.time;
            }

            m_isGrounded = (Time.time - m_lastGroundTime) <= 0.1f;

            return m_isGrounded;
        }

        public void ApplyGravity()
        {
            if (m_isGrounded && m_currentVelocityY < 0)
            {
                m_currentVelocityY = -2f;
                return;
            }

            m_currentVelocityY += m_gravityPower * Time.deltaTime;

            if (m_currentVelocityY < 0)
            {
                m_currentVelocityY += m_gravityPower * (m_fallMultiplier - 1) * Time.deltaTime;
            }
        }

        // ==================== Jump 
        public void JumpUpStart(Vector3 inputDir)
        {
            // Ground 무시 시작
            OnLeaveGround(m_jumpIgnoreGroundTime);

            // 수평 방향은 "입력 기반"으로 직접 결정
            m_currentVelocityXZ = inputDir * m_baseMoveSpeed * m_jumpDirshorten;

            // 수직 속도 설정
            m_currentVelocityY = Mathf.Sqrt(m_jumpPower * -2f * m_gravityPower);
        }

        public void Fall()
        {

        }
        // ==================== Dash 
        
    }
}