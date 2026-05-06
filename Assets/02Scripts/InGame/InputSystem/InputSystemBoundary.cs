// Boundary : 입력/외부 이벤트
using UnityEngine;
using UnityEngine.InputSystem;

// 입력(외부) 신호 이벤트에 대한 전달을 받아들이는 클래스
namespace alpha.player.boundary
{
    public class InputSystemBoundary : MonoBehaviour
    {
        private InputActionSystem m_inputAction;

        #region Player
        //===== LocomotionInput
        public Vector2 MoveInputDir { get; private set; }
        public bool IsJumpInput => m_jumpFrame == Time.frameCount;   // 다음 프레임에서 false로 변환해줌
        private int m_jumpFrame;

        public bool IsDashInput => m_dashFrame == Time.frameCount;
        private int m_dashFrame;

        public bool IsFlyInput => m_flyFrame == Time.frameCount;
        private int m_flyFrame;
        //===== CombatInput

        #endregion

        #region Camera
        public Vector2 LookInputDir { get; private set; }

        #endregion
        void OnEnable()
        {
            if (m_inputAction == null)
            {
                m_inputAction = new InputActionSystem();

                m_inputAction.Player.Move.performed += i => MoveInputDir = i.ReadValue<Vector2>();
                m_inputAction.Player.Move.canceled += i => MoveInputDir = Vector2.zero;

                m_inputAction.Camera.Look.performed += i => LookInputDir = i.ReadValue<Vector2>();
                m_inputAction.Camera.Look.canceled += i => LookInputDir = Vector2.zero;

                m_inputAction.Player.Jump.performed += i => m_jumpFrame = Time.frameCount;

                m_inputAction.Player.Dash.performed += i => m_dashFrame = Time.frameCount;

                m_inputAction.Player.Fly.performed += i => m_flyFrame = Time.frameCount;
                // 활성화해야 동작
                m_inputAction.Enable();
            }
        }
    }
}