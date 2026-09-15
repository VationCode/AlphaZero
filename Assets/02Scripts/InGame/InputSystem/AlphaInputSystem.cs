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
        public bool IsDodge => _dodgeFrame == Time.frameCount;
        private int _dodgeFrame;

        public bool IsJumpInput => _jumpFrame == Time.frameCount;   // 다음 프레임에서 false로 변환해줌
        private int _jumpFrame;


        public bool IsDash => _dashFrame == Time.frameCount;
        private int _dashFrame;

        public bool IsFlyInput => _flyFrame == Time.frameCount;
        private int _flyFrame;
        //===== CombatInput
        public int SwapNum { get; private set; }
        public bool IsSwapInput => _swapFrame == Time.frameCount;
        private int _swapFrame;

        public bool IsAttack {get; private set; }

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

                _inputAction.Player.Sprint.performed += i => IsSprint = true;
                _inputAction.Player.Sprint.canceled += i => IsSprint = false;

                _inputAction.Player.Dodge.performed += i => _dodgeFrame = Time.frameCount;
                _inputAction.Player.Jump.performed += i => _jumpFrame = Time.frameCount;
                _inputAction.Player.Dash.performed += i => _dashFrame = Time.frameCount;
                _inputAction.Player.Fly.performed += i => _flyFrame = Time.frameCount;

                // Camera
                _inputAction.Camera.Look.performed += i => LookInputDir = i.ReadValue<Vector2>();
                _inputAction.Camera.Look.canceled += i => LookInputDir = Vector2.zero;

                // Combat
                _inputAction.Player.Swap.performed += OnSwap;

                _inputAction.Player.Attack.performed += i => IsAttack = true;
                _inputAction.Player.Attack.canceled += i => IsAttack = false;


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
                _swapFrame = Time.frameCount;
            }
        }
    }
}