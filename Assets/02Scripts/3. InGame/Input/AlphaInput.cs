using UnityEngine;
using UnityEngine.InputSystem;

namespace alpha.ingame.input
{
    public class AlphaInput : MonoBehaviour
    {
        public static AlphaInput Instance { get; private set; }

        private AlphaInputAction _action;

        // Locomotion
        public Vector2 MoveInput => _moveInput;
        Vector2 _moveInput;

        public bool IsWalk => _isWalk;
        bool _isWalk;

        public bool IsJump => _jumpFrame == Time.frameCount;  // 한 프레임 단위만 True로 이후 False
        private int _jumpFrame = -1;

        public bool IsDodge => _dodgeFrame == Time.frameCount;
        private int _dodgeFrame = -1;

        // 기동: 눌림과 유지 입력은 아레테별로 다르게 해석한다.
        public bool IsMobilityPressed => _mobilityFrame == Time.frameCount;
        public bool IsMobilityHeld => _isMobilityHeld;
        private bool _isMobilityHeld;
        private int _mobilityFrame = -1;

        public bool IsCombat => !_isQuickSlotOpen && _isCombat;
        private bool _isCombat;

        // Interaction
        public bool IsQuickSlotPressed => _quickSlotFrame == Time.frameCount;
        public bool IsQuickSlotOpen => _isQuickSlotOpen;
        private int _quickSlotFrame = -1;
        private bool _isQuickSlotOpen;

        // Camera
        public Vector2 LookInput => _isQuickSlotOpen ? Vector2.zero : _lookInput;
        private Vector2 _lookInput;

        public Vector2 ZoomInput => _zoomInput;
        private Vector2 _zoomInput;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            _action = new AlphaInputAction();
            // 생성된 입력 래퍼가 갱신되기 전에도 Tab 바인딩을 적용한다.
            _action.PlayerInteraction.QuickSlot.ApplyBindingOverride("<Keyboard>/tab");

            // Locomotion
            _action.PlayerLocomotion.Move.performed += i => _moveInput = i.ReadValue<Vector2>();
            _action.PlayerLocomotion.Move.canceled += i => _moveInput = Vector2.zero;

            _action.PlayerLocomotion.Walk.performed += _ => _isWalk = !_isWalk;

            _action.PlayerLocomotion.Jump.performed += i => _jumpFrame = Time.frameCount;
            _action.PlayerLocomotion.Mobility.performed += OnMobilityPerformed;
            _action.PlayerLocomotion.Mobility.canceled += OnMobilityCanceled;
            _action.PlayerLocomotion.Dodge.performed += i => _dodgeFrame = Time.frameCount;

            // Combat
            _action.PlayerCombat.Attack.performed += OnAttackPerformed;
            _action.PlayerCombat.Attack.canceled += OnAttackCanceled;

            // Interaction
            _action.PlayerInteraction.QuickSlot.performed += OnQuickSlotPerformed;

            // Camera
            _action.Camera.Look.performed += i => _lookInput = i.ReadValue<Vector2>();
            _action.Camera.Look.canceled += i => _lookInput = Vector2.zero;

            _action.Camera.Zoom.performed += i => _zoomInput = i.ReadValue<Vector2>();
            _action.Camera.Zoom.canceled += i => _zoomInput = Vector2.zero;


            _action.Enable();
        }

        private void OnDisable()
        {
            _action.PlayerLocomotion.Move.performed -= i => _moveInput = i.ReadValue<Vector2>();
            _action.PlayerLocomotion.Move.canceled -= i => _moveInput = Vector2.zero;

            _action.PlayerLocomotion.Walk.performed -= i => _isWalk = !_isWalk;

            _action.PlayerLocomotion.Jump.performed -= i => _jumpFrame = Time.frameCount;

            _action.PlayerLocomotion.Mobility.performed -= OnMobilityPerformed;
            _action.PlayerLocomotion.Mobility.canceled -= OnMobilityCanceled;

            _action.PlayerLocomotion.Dodge.performed -= i => _dodgeFrame = Time.frameCount;

            // Interaction
            _action.PlayerInteraction.QuickSlot.performed -= OnQuickSlotPerformed;

            // Combat
            _action.PlayerCombat.Attack.performed -= OnAttackPerformed;
            _action.PlayerCombat.Attack.canceled -= OnAttackCanceled;



            // Camera
            _action.Camera.Look.performed -= i => _lookInput = i.ReadValue<Vector2>();
            _action.Camera.Look.canceled -= i => _lookInput = Vector2.zero;

            _action.Camera.Zoom.performed -= i => _zoomInput = i.ReadValue<Vector2>();
            _action.Camera.Zoom.canceled -= i => _zoomInput = Vector2.zero;



            _action.Disable();

            _action = null;
            _isCombat = false;
            _isQuickSlotOpen = false;
            _quickSlotFrame = -1;
            _isMobilityHeld = false;
        }

        private void OnMobilityPerformed(InputAction.CallbackContext context)
        {
            _mobilityFrame = Time.frameCount;
            _isMobilityHeld = true;
        }

        private void OnMobilityCanceled(InputAction.CallbackContext context)
        {
            _isMobilityHeld = false;
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            if (!_isQuickSlotOpen) _isCombat = true;
        }

        private void OnAttackCanceled(InputAction.CallbackContext context)
        {
            _isCombat = false;
        }

        private void OnQuickSlotPerformed(InputAction.CallbackContext context)
        {
            _quickSlotFrame = Time.frameCount;
        }

        public void SetQuickSlotOpen(bool isOpen)
        {
            _isQuickSlotOpen = isOpen;
            _lookInput = Vector2.zero;
            if (!isOpen) return;

            // 열리는 순간 이미 누르고 있던 공격과 마우스 회전량도 지운다.
            _isCombat = false;
        }

    }
}
