// Boundary : 외부 이벤트와의 연결점
using UnityEngine;
using UnityEngine.InputSystem;

// 입력(외부) 신호 이벤트에 대한 전달 클래스
namespace alpha.player.boundary
{
    public class PlayerInputBoundary : MonoBehaviour
    {
        private PlayerInputAction m_inputAction;

        #region LocomotionInput
        public Vector2 MoveInputDir { get; private set; }
        public Vector2 LookInputDir { get; private set; }
        #endregion

        #region CombatInput

        #endregion
        void OnEnable()
        {
            if (m_inputAction == null)
            {
                m_inputAction = new PlayerInputAction();

                m_inputAction.Player.Move.performed += i => MoveInputDir = i.ReadValue<Vector2>();
                m_inputAction.Player.Move.canceled += i => MoveInputDir = Vector2.zero;

                m_inputAction.Player.Look.performed += i => LookInputDir = i.ReadValue<Vector2>();
                m_inputAction.Player.Look.canceled += i => LookInputDir = Vector2.zero;

                // 활성화해야 동작
                m_inputAction.Enable();
            }
        }
    }
}