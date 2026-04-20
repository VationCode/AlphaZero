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
        private Vector2 m_currentDir;
        #endregion
        public void Bind(PlayerInputBoundary playerInputBoundary)
        {
            m_inputBoundary = playerInputBoundary;
        }

        public void Move()
        {
            m_currentDir = m_inputBoundary.MoveDirection;
            m_currentMoveSpeed = m_currentDir.y < 0 ? m_backMoveSpeed : m_baseMoveSpeed;

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