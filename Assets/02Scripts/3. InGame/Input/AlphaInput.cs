using UnityEngine;

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

        public bool IsDash => _isDash;
        private bool _isDash => _dashFrame == Time.frameCount;
        private int _dashFrame = -1;

        public bool IsCombat => _isCombat;
        private bool _isCombat;

        // Camera
        public Vector2 LookInput => _lookInput;
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

            // Locomotion
            _action.PlayerLocomotion.Move.performed += i => _moveInput = i.ReadValue<Vector2>();
            _action.PlayerLocomotion.Move.canceled += i => _moveInput = Vector2.zero;

            _action.PlayerLocomotion.Walk.performed += _ => _isWalk = !_isWalk;

            _action.PlayerLocomotion.Jump.performed += i => _jumpFrame = Time.frameCount;
            _action.PlayerLocomotion.Dash.performed += i => _dashFrame = Time.frameCount;
            _action.PlayerLocomotion.Dodge.performed += i => _dodgeFrame = Time.frameCount;

            // Combat
            _action.PlayerCombat.Attack.performed += i => _isCombat = true;
            _action.PlayerCombat.Attack.canceled += i => _isCombat = false;

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
            _action.PlayerLocomotion.Dash.performed -= i => _dashFrame = Time.frameCount;
            _action.PlayerLocomotion.Dodge.performed -= i => _dodgeFrame = Time.frameCount;

            // Combat
            _action.PlayerCombat.Attack.performed -= i => _isCombat = true;
            _action.PlayerCombat.Attack.canceled -= i => _isCombat = false;

            // Camera
            _action.Camera.Look.performed -= i => _lookInput = i.ReadValue<Vector2>();
            _action.Camera.Look.canceled -= i => _lookInput = Vector2.zero;

            _action.Camera.Zoom.performed -= i => _zoomInput = i.ReadValue<Vector2>();
            _action.Camera.Zoom.canceled -= i => _zoomInput = Vector2.zero;

            _action.Disable();

            _action = null;
        }
    }
}