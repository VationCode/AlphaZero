// Module : 기능들의 집합체
using player.boundary;
using UnityEngine;

namespace player.module
{
    [RequireComponent(typeof(CharacterController))]
    // 플레이어의 이동과 관련된 기능들을 담당하는 모듈
    public class PlayerLocomotionModule : MonoBehaviour
    {
        #region Ref Component
        [SerializeField] private CharacterController m_characterController; // 물리영향거의x, 직접스크립트제어 방식
        private PlayerInputBoundary m_inputBoundary;
        #endregion

        #region Config 
        [Header("[ Move ]")]
        [SerializeField] private float m_baseMoveSpeed;
        [SerializeField] private float m_backMoveSpeed;
        [SerializeField] private float m_combatMoveSpeed;

        [Header("[ rotation ]")]
        [SerializeField] private float m_rotationSpeed;

        [Header("[ Jump ]")]
        [SerializeField] private float m_jumpPower;

        [Header("[ Dash ]")]
        [SerializeField] private float m_dashPower;

        [Header("[ Fly ]")]
        [SerializeField] private float m_flyUpPower;

        [Header("[ Gravity ]")]
        [SerializeField] private float m_gravityPower;
        #endregion

        #region Runtime
        private float m_currentMoveSpeed;
        private Vector3 m_currentDir;
        private Vector3 m_currentVelocity;
        #endregion

        private void Awake()
        {
            m_characterController = GetComponent<CharacterController>();
        }

        public void Bind(PlayerInputBoundary playerInputBoundary)
        {
            m_inputBoundary = playerInputBoundary;
        }

        public void Move()
        {
            // 방향 계산
            Vector2 _moveInputDir = m_inputBoundary.MoveInputDir;
            Vector3 _Dir = transform.forward * _moveInputDir.y + transform.right * _moveInputDir.x;
            if (_Dir.sqrMagnitude < 0.01f) _Dir = Vector2.zero;
            else _Dir = Vector3.ClampMagnitude(_Dir, 1f);
            m_currentDir = _Dir;

            // 속력 계산
            m_currentMoveSpeed = m_currentDir.y < 0 ? m_backMoveSpeed : m_baseMoveSpeed;

            // 속도 계산
            m_currentVelocity = m_currentDir * m_currentMoveSpeed;

            // 이동 적용
            m_characterController.Move(m_currentVelocity * Time.deltaTime);
        }

        private void Update()
        {
            Move();
        }

        public void Rotation()
        {

        }

        public void Jump()
        {

        }

        public void Dash()
        {

        }
    }
}