// Boundary : 입력/외부 이벤트
using UnityEngine;
using UnityEngine.InputSystem;

// 입력(외부) 신호 이벤트에 대한 전달을 받아들이는 클래스
namespace alpha.input
{
    public class AlphaInputSystem : MonoBehaviour
    {
        private InputActionSystem _inputAction;

        #region Player
        //===== LocomotionInput
        public Vector2 MoveInputDir { get; private set; }
        public bool IsSprint;

        public bool IsDodge => m_dodgeFrame == Time.frameCount;
        private int m_dodgeFrame;

        public bool IsJumpInput => m_jumpFrame == Time.frameCount;   // 다음 프레임에서 false로 변환해줌
        private int m_jumpFrame;


        public bool IsDashInput => m_dashFrame == Time.frameCount;
        private int m_dashFrame;

        public bool IsFlyInput => m_flyFrame == Time.frameCount;
        private int m_flyFrame;
        //===== CombatInput
        public int SwapNum { get; private set; }
        public bool IsSwapInput => m_swapFrame == Time.frameCount;
        private int m_swapFrame;

        public bool IsAttackInput {get; private set; }

        #endregion

        #region Camera
        public Vector2 LookInputDir { get; private set; }

        #endregion
        void OnEnable()
        {
            if (_inputAction == null)
            {
                _inputAction = new InputActionSystem();

                // Locomotion
                _inputAction.Player.Move.performed += i => MoveInputDir = i.ReadValue<Vector2>();
                _inputAction.Player.Move.canceled += i => MoveInputDir = Vector2.zero;

                _inputAction.Player.Dodge.performed += i => m_dodgeFrame = Time.frameCount;

                _inputAction.Player.Sprint.performed += i =>
                {
                    if (IsDodge)
                    {
                        IsSprint = false;
                        return;
                    }
                    IsSprint = true;
                };

                _inputAction.Player.Sprint.canceled += i => IsSprint = false;

                _inputAction.Camera.Look.performed += i => LookInputDir = i.ReadValue<Vector2>();
                _inputAction.Camera.Look.canceled += i => LookInputDir = Vector2.zero;

                _inputAction.Player.Jump.performed += i =>
                {
                    if(IsDodge)
                    {
                        m_jumpFrame = 0;
                        return;
                    }
                    m_jumpFrame = Time.frameCount;
                };


                _inputAction.Player.Dash.performed += i => m_dashFrame = Time.frameCount;

                _inputAction.Player.Fly.performed += i => m_flyFrame = Time.frameCount;

                // Combat
                _inputAction.Player.Swap.performed += OnSwap;

                _inputAction.Player.Attack.performed += i => IsAttackInput = true;
                _inputAction.Player.Attack.canceled += i => IsAttackInput = false;


                // 활성화해야 동작
                _inputAction.Enable();
            }
        }

        // Numpad 대응
        private void OnSwap(InputAction.CallbackContext p_context)
        {
            string key = p_context.control.displayName;

            if (int.TryParse(key, out int number))
            {
                SwapNum = number - 1;
                m_swapFrame = Time.frameCount;
            }
        }
    }
}