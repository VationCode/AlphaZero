using UnityEngine;

namespace alpha.ingame.player
{
    public class AnimationView : MonoBehaviour
    {
        [Header("Ref")]
        [SerializeField] private Transform _characterRoot;
        [SerializeField] private Animator _animator;
        [SerializeField] private RuntimeAnimatorController[] _animCtrls;

        [Header("Blend Time")]
        [SerializeField] private float _moveBlendTime = 0.12f;

        // Animator 상태
        private readonly int _moveTree = Animator.StringToHash("Base Layer.Move Tree");
        private readonly int _combatMoveTree = Animator.StringToHash("Base Layer.CombatMove Tree");

        private readonly int _jump = Animator.StringToHash("Base Layer.Jump");
        private readonly int _doubleJump = Animator.StringToHash("Base Layer.Double Jump");
        private readonly int _runJump = Animator.StringToHash("Base Layer.Run Jump");
        private readonly int _fall = Animator.StringToHash("Base Layer.Fall");
        private readonly int _land = Animator.StringToHash("Base Layer.Land");

        // Blend Tree 파라미터
        private readonly int _inputX = Animator.StringToHash("InputX");
        private readonly int _inputY = Animator.StringToHash("InputY");
        private readonly int _moveSpeed = Animator.StringToHash("MoveSpeed");

        private int _currentMoveState;

        private void Awake()
        {
            if(_animator == null) _animator = GetComponent<Animator>();

            if (_characterRoot == null)
            {
                _characterRoot = GetComponentInParent<CharacterController>().transform;
            }
        }

        public void HandlePlayerSetup(EAreteType p_type)
        {
            int index = (int)p_type;

            if (_animator == null || _animCtrls == null ||
                index < 0 || index >= _animCtrls.Length ||
                _animCtrls[index] == null)
                return;

            _animator.runtimeAnimatorController = _animCtrls[index];

            _currentMoveState = 0;
        }

        public void MoveAnim(Vector3 p_moveDirection, EMoveType p_moveType, float p_currentSpeed)
        {
            if (_animator == null || !_animator.isActiveAndEnabled ||
                _animator.runtimeAnimatorController == null)
                return;

            int nextState = p_moveType == EMoveType.Combat ? _combatMoveTree : _moveTree;


            if (p_moveType == EMoveType.Combat)
            {
                // Combat Tree에는 캐릭터 기준 방향을 전달한다.
                Vector3 localDirection = _characterRoot != null ? 
                                         _characterRoot.InverseTransformDirection(p_moveDirection) : p_moveDirection;

                _animator.SetFloat(_inputX, p_moveDirection.x);
                _animator.SetFloat(_inputY, p_moveDirection.z);
                return;
            }
            else
                _animator.SetFloat(_moveSpeed, p_currentSpeed);

            if (_currentMoveState == nextState)
                return;

            _currentMoveState = nextState;
            _animator.CrossFadeInFixedTime(nextState, _moveBlendTime, 0, 0f);
        }

        public void JumpAnim(EMoveType p_moveType, int p_count)
        {
            int state = p_count == 2 ? _doubleJump : p_moveType == EMoveType.Run ? _runJump : _jump;

            _currentMoveState = 0;
            _animator.CrossFadeInFixedTime(state, 0.08f, 0, 0f); // 점프 시작부터 재생
        }


        public void FallAnim()
        {
            _currentMoveState = 0;

            _animator.CrossFadeInFixedTime(_fall, 0.059263706f, 0, 0.7273034f * 1.2f);
        }

        public void LandAnim()
        {
            if (_animator == null || !_animator.isActiveAndEnabled ||
        !_animator.HasState(0, _land))
                return;

            _currentMoveState = 0;
            _animator.CrossFadeInFixedTime(_land, _moveBlendTime, 0, 0f);
        }

        public bool IsLandAnimFinished()
        {
            if (_animator == null || !_animator.isActiveAndEnabled ||
                !_animator.HasState(0, _land))
                return true;

            if (_animator.IsInTransition(0))
                return false;

            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            return state.fullPathHash == _land && state.normalizedTime >= 1f;
        }

        private void OnAnimatorMove()
        {
            // 이동과 회전은 LocomotionModule이 처리한다.
        }

    }
}
