// Module : 기능 수행
using alpha.player.boundary;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

namespace alpha.player.module
{
    [RequireComponent(typeof(CharacterController))]
    // 플레이어의 이동과 관련된 기능들을 담당하는 모듈
    public class PlayerLocomotionModule : MonoBehaviour
    {
        #region Ref Component
        [SerializeField] private CharacterController m_characterController; // 물리영향거의x, 직접스크립트제어 방식
        private InputSystemBoundary m_inputBoundary;
        private PlayerAnimationBoundary m_aniBoundary;
        #endregion

        #region Config 
        [Header("[ Move ]")]
        [SerializeField] private float m_baseMoveSpeed;
        // [SerializeField] private float m_backMoveSpeed; 현재는 단순화로 진행하여 이후 애니메이션 늘리기
        [SerializeField] private float m_combatMoveSpeed;
        private float m_moveAnismoothTime = 0.1f;

        [Header("[ rotation ]")]
        [SerializeField] private float m_rotationSpeed;
        private float m_rotationsmoothTime = 0.1f;

        [Header("[ Ground ]")]
        [SerializeField]
        private float m_groundDistance;
        [SerializeField]
        private LayerMask m_groundMask;

        [Header("[ Jump ]")]
        [SerializeField] 
        private float m_jumpPower;

        [Header("[ Dash ]")]
        [SerializeField] 
        private float m_dashPower;

        [Header("[ Fly ]")]
        [SerializeField] 
        private float m_flyUpPower;



        [Header("[ Gravity ]")]
        [SerializeField] private float m_gravityPower;
        #endregion

        #region Runtime
        // Move
        private float m_currentMoveSpeed;
        private Vector3 m_currentDir;
        private Vector3 m_currentVelocity;
        private float m_moveAniMagnitude;
        private float m_moveAniVelocity;
        //Rotation
        private float m_rotationSmoothVelocity;

        // Ground
        private bool m_isGrounded;
        private float m_lastGroundTime;
        // Jump

        #endregion

        private void Awake()
        {
            m_characterController = GetComponent<CharacterController>();
        }

        public void Bind(InputSystemBoundary inputBoundary, PlayerAnimationBoundary aniBoundary)
        {
            m_inputBoundary = inputBoundary;
            m_aniBoundary = aniBoundary;
        }

        public Vector3 Move(bool isCombat)
        {
            // 카메라기준으로 캐릭터 이동
            Vector3 _forward = Camera.main.transform.forward;
            _forward.y = 0f;    // y값에 따라 높이가 변해버리기에 0으로 설정
            Vector3 _right = Camera.main.transform.right;

            // 방향 계산
            Vector2 _moveInputDir = m_inputBoundary.MoveInputDir;
            Vector3 _Dir = _forward * _moveInputDir.y + _right * _moveInputDir.x;

            // 값 보정
            if (_Dir.sqrMagnitude < 0.01f) _Dir = Vector2.zero;
            else _Dir = Vector3.ClampMagnitude(_Dir, 1f);

            // 속력 계산
            float _speed = isCombat ? m_combatMoveSpeed : m_baseMoveSpeed;

            // 속도 계산
            Vector3 _velocity = _Dir * _speed;

            if(!m_isGrounded)
            {
                _velocity = Vector3.zero;
            }

            m_currentDir = _Dir;
            m_currentMoveSpeed = _speed;

            m_moveAniMagnitude = Mathf.SmoothDamp(m_moveAniMagnitude, _velocity.magnitude, ref m_moveAniVelocity,m_moveAnismoothTime);
            m_aniBoundary.SetMove(m_moveAniMagnitude);

            return _velocity;
        }
        public void Rotation()
        {
            if (m_currentDir == Vector3.zero) return;
            if(!m_isGrounded) return;

            // 이동 방향에 대한 회전값 반환
            Quaternion targetRot = Quaternion.LookRotation(m_currentDir);

            // 목표 회전의 Y각도 추출(지상은 y축만 필요)
            float _targetAngle = targetRot.eulerAngles.y;

            float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetAngle, ref m_rotationSmoothVelocity, m_rotationsmoothTime);

            // 캐릭터 회전 적용
            transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
        }

        private void Update()
        {
            CheckedGround();

            Vector3 _horizontal = Move(false);
            float _vertical = ApplyGravity();

            Vector3 _finalVelocity = _horizontal + Vector3.up * _vertical;

            m_characterController.Move(_finalVelocity * Time.deltaTime);

            Rotation();

            
        }

        public void CheckedGround()
        {
            // m_characterController.center 바닥에서 조금 띄어져있는 상태
            Vector3 worldCenter = m_characterController.transform.TransformPoint(m_characterController.center);
            float _height = m_characterController.height;

            Vector3 _colliderButtomtr = worldCenter - Vector3.up * (m_characterController.height * 0.5f - m_characterController.skinWidth);

            bool _groundCheck = Physics.CheckSphere(_colliderButtomtr, m_groundDistance, m_groundMask);
            // 점프같은 상태에서 바로 체크를 하면 True가 나온 후 False가 나오기에
            // 프레임 단위로 약간의 시간차를 두고 체크
            if (_groundCheck) m_lastGroundTime = Time.time;

            m_isGrounded = (Time.time - m_lastGroundTime) <= 0.1f;
        }

        public float ApplyGravity()
        {
            if (m_isGrounded && m_currentVelocity.y < 0)
            {
                m_currentVelocity.y = -2f; // 바닥에 붙이기용
            }
            else
            {
                m_currentVelocity.y += m_gravityPower * Time.deltaTime;
            }

            return m_currentVelocity.y;
        }

        public void Jump()
        {
            
        }

        public void Dash()
        {

        }
    }
}