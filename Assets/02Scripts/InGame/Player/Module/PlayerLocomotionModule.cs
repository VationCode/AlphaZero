// Module : 기능 연산 위주
using UnityEngine;

namespace alpha.player.locomotion
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

        [Header("[ rotation ]")]
        [SerializeField] private float m_rotationsmoothTime = 0.1f;

        [Header("[ Jump ]")]
        [SerializeField]
        private float m_jumpPower = 3f;
        // 점프 시 수평 이동 속도 보정값 (0.1~1 사이)
        [SerializeField, Range(0.1f, 1)]
        private float m_jumpDirshorten = 0.5f;
        private float m_jumpIgnoreGroundTime = 0.2f;

        [Header("[ Fall ]")]
        [SerializeField]
        private float m_fallMultiplier = 2f;    // 중력 가속도 보정값 (1보다 커야 빠르게 떨어짐)

        [Header("[ Dash ]")]
        // 대쉬는 속도보단 시간과 거리에 초점이 맞춰지는게 좋음
        [SerializeField]
        private float m_dashDistance = 10;
        [SerializeField]
        private float m_dashDuration = 0.5f;

        // 대쉬와 동일하게 Vertical방향으로
        [Header("[ Fly ]")]
        [SerializeField]
        private float m_flyUpDistance = 10;
        [SerializeField]
        private float m_flyUpDuration = 0.5f;
        [SerializeField]
        private float m_flightMoveSpeed = 15;

        [Header("[ Gravity ]")]
        [SerializeField] private float m_gravityPower = -9.8f;
        #endregion

        #region Runtime
        // Move
        private float m_currentMoveSpeed;
        private Vector3 m_currentDir;
        private Vector3 m_currentVelocityXZ;
        private float m_currentVelocityY;

        // Ground
        private bool m_isGrounded;
        private float m_lastGroundTime;
        private float m_lastLeaveGroundTime;
        private float m_groundIgnoreDuration;

        //Rotation
        private float m_rotationSmoothVelocity;

        // Dash
        private float m_currentDashDistance;

        // Fly
        private float m_currentFlyUpDistance;
        #endregion

        private void Start()
        {
            m_isGrounded = true;
        }

        public Vector3 GetFinalVelocity()
        {
            return m_currentVelocityXZ + Vector3.up * m_currentVelocityY;
        }
        public void SetHorizontalVelocity(Vector3 p_velocity)
        {
            m_currentVelocityXZ = p_velocity;
        }
        public Vector3 GetLastDirection()
        {
            return m_currentDir;
        }

        public void HandleMove(bool p_isCombat, Vector2 p_moveInputDir)
        {
            // 카메라기준으로 캐릭터 이동
            Vector3 forward = Camera.main.transform.forward;
            forward.y = 0f;    // y값에 따라 높이가 변해버리기에 0으로 설정
            Vector3 right = Camera.main.transform.right;

            // 방향 계산
            Vector2 moveInputDir = p_moveInputDir;
            Vector3 moveDir = forward * moveInputDir.y + right * moveInputDir.x;

            // 값 보정
            if (moveDir.sqrMagnitude < 0.01f) moveDir = Vector2.zero;
            else moveDir = Vector3.ClampMagnitude(moveDir, 1f);

            // 속력 계산
            float speed = p_isCombat ? m_combatMoveSpeed : m_baseMoveSpeed;

            // 속도 계산
            Vector3 velocity = moveDir * speed;

            m_currentDir = moveDir;
            m_currentMoveSpeed = speed;
            m_currentVelocityXZ = velocity;
        }

        public void HandleRotation(bool p_instant = false)
        {
            if (m_currentDir == Vector3.zero) return;

            Quaternion targetRot = Quaternion.LookRotation(m_currentDir);

            if (p_instant)
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
        public void OnLeaveGround(float p_ignoreTime)
        {
            m_lastLeaveGroundTime = Time.time;
            m_groundIgnoreDuration = p_ignoreTime;
        }

        public bool UpdateGround(bool p_isGroundDetected)
        {
            if ((Time.time - m_lastLeaveGroundTime) < m_groundIgnoreDuration)
            {
                m_isGrounded = false;
                return m_isGrounded;
            }

            if (p_isGroundDetected)
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
        public void SetupJump(Vector3 p_inputDir)
        {
            // Ground 무시 시작
            OnLeaveGround(m_jumpIgnoreGroundTime);

            // 입력 방향으로 즉시 회전
            HandleRotation(true);

            // 수평 방향은 "입력 기반"으로 직접 결정
            m_currentVelocityXZ = p_inputDir * m_baseMoveSpeed * m_jumpDirshorten;

            // 수직 속도 설정
            m_currentVelocityY = Mathf.Sqrt(m_jumpPower * -2f * m_gravityPower);
        }
        // ==================== Fall
        public void SetupFall()
        {
            // 현재 바라보는 방향을 수평으로 보정
            Vector3 dir = transform.forward;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.01f)
            {
                dir.Normalize();
                m_currentDir = dir;
                HandleRotation(true);
            }
        }

        // ==================== Dash 
        public void SetupDash(Vector3 p_inputDir)
        {
            if (p_inputDir.sqrMagnitude < 0.01f)
                p_inputDir = transform.forward;

            p_inputDir.y = 0f;
            p_inputDir.Normalize();

            m_currentDir = p_inputDir;

            HandleRotation(true);

            m_currentDashDistance = 0;
        }

        public bool UpdateDash()
        {
            float dashSpeed = m_dashDistance / m_dashDuration;

            // 프레임당 이동 단위
            float moveStep = dashSpeed * Time.deltaTime;
            bool isDashing = true;

            m_currentDashDistance += moveStep;

            // 거리 초과 방지
            if (m_currentDashDistance >= m_dashDistance)
            {
                moveStep -= (m_currentDashDistance - m_dashDistance);
                isDashing = false;
            }

            m_currentVelocityXZ = m_currentDir * (moveStep / Time.deltaTime);
            return isDashing;
        }

        // ==================== Fly
        public void SetupFlyUp()
        {
            m_currentFlyUpDistance = 0;
        }

        public bool UpdateFlyUp()
        {
            float flyUpSpeed = m_flyUpDistance / m_flyUpDuration;
            float moveStep = flyUpSpeed * Time.deltaTime;
            bool isRising = true;

            m_currentFlyUpDistance += moveStep;

            if (m_currentFlyUpDistance >= m_flyUpDistance)
            {
                moveStep -= (m_currentFlyUpDistance - m_flyUpDistance);
                isRising = false;
            }

            m_currentVelocityY = moveStep / Time.deltaTime;

            return isRising;
        }
        public void HandleFlightMove(Vector2 p_moveDir)
        {
            Transform cam = Camera.main.transform;

            Vector3 forward = cam.forward;   // y 포함
            Vector3 right = cam.right;

            Vector3 dir = forward * p_moveDir.y + right * p_moveDir.x;

            // 정규화
            if (dir.sqrMagnitude < 0.01f)
                dir = Vector3.zero;
            else
                dir.Normalize();

            float speed = m_flightMoveSpeed;

            m_currentDir = dir;
            m_currentVelocityXZ = new Vector3(dir.x, 0, dir.z) * speed;
            m_currentVelocityY = dir.y * speed;

        }
        public void HandleFlightRotation()
        {
            if (m_currentDir.sqrMagnitude < 0.01f) return;

            Quaternion targetRot = Quaternion.LookRotation(m_currentDir);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * 10f
            );
        }
    }
}