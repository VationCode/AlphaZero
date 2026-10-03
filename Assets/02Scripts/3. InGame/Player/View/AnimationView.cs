using UnityEngine;

namespace alpha.ingame.player
{
    public class AnimationView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _characterRoot;
        [SerializeField] private Animator _animator;
        [SerializeField] private RuntimeAnimatorController[] _animCtrls;

        [Header("Blend Time")]
        [SerializeField] private float _moveBlendTime = 0.12f;

        // Animation Layer Names
        private readonly int _idle = Animator.StringToHash("Base Layer.Idle");
        private readonly int _walk = Animator.StringToHash("Base Layer.Walk");
        private readonly int _jog = Animator.StringToHash("Base Layer.Jog");
        private readonly int _sprint = Animator.StringToHash("Base Layer.Sprint");
        private readonly int _combatMoveTree = Animator.StringToHash("Base Layer.CombatMove Tree");

        private readonly int _inputX = Animator.StringToHash("InputX");
        private readonly int _inputY = Animator.StringToHash("InputY");
        private readonly int _walkTurn = Animator.StringToHash("Base Layer.Walk Turn");
        private readonly int _jogTurn = Animator.StringToHash("Base Layer.Jog Turn");
        private readonly int _sprintTurn = Animator.StringToHash("Base Layer.Sprint Turn");

        private int _currentMoveState;
        private int _currentTurnState;
        private bool _useTurnRootMotion;
        private bool _turnStateSeen;
        public bool IsReady => _animator != null && _animator.isActiveAndEnabled
                                && _animator.runtimeAnimatorController != null;

        private readonly int _turnTrigger = Animator.StringToHash("Turn");
        private readonly int _turnType = Animator.StringToHash("TurnType");

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
            _currentTurnState = 0;
            _turnStateSeen = false;
        }

        public void MoveAnim(Vector3 p_moveDirection, EMoveType p_moveType)
        {
            if (!IsReady || _useTurnRootMotion) return;

            int nextState = p_moveType switch
            {
                EMoveType.Idle => _idle,
                EMoveType.Walk => _walk,
                EMoveType.Jog => _jog,
                EMoveType.Sprint => _sprint,
                EMoveType.Combat => _combatMoveTree,
                _ => _idle
            };

            if (!_animator.HasState(0, nextState)) return;

            if (p_moveType == EMoveType.Combat)
            {
                if (_characterRoot == null) return;

                // Combat Tree에는 캐릭터 기준 이동 방향을 전달한다.
                Vector3 localDirection = _characterRoot.InverseTransformDirection(p_moveDirection);

                Vector2 blend = Vector2.ClampMagnitude(new Vector2(localDirection.x, localDirection.z), 1f);

                _animator.SetFloat(_inputX, blend.x);
                _animator.SetFloat(_inputY, blend.y);
            }

            // 일반 이동 상태가 바뀔 때만 블렌딩한다.
            if (_currentMoveState == nextState) return;

            _currentMoveState = nextState;
            _animator.CrossFadeInFixedTime(nextState, _moveBlendTime, 0, 0f);
        }
        public bool CanTurn(EMoveType p_moveType)
        {
            int turnState = GetTurnState(p_moveType);
            return IsReady && _characterRoot != null &&
                   turnState != 0 && _animator.HasState(0, turnState);
        }

        private int GetTurnState(EMoveType p_moveType)
        {
            switch (p_moveType)
            {
                case EMoveType.Walk: return _walkTurn;
                case EMoveType.Jog: return _jogTurn;
                case EMoveType.Sprint: return _sprintTurn;
                default: return 0; // Idle과 Combat Turn 없음
            }
        }

        public void TurnAnim(EMoveType p_moveType)
        {
            if (!CanTurn(p_moveType)) return;

            int turnState = GetTurnState(p_moveType);

            // Animator에 연결한 Transition을 통해 Turn으로 진입한다.
            SetTurnRootMotion(true);
            _currentTurnState = turnState;
            _currentMoveState = 0;
            _turnStateSeen = false;

            _animator.SetInteger(_turnType, (int)p_moveType);
            _animator.ResetTrigger(_turnTrigger);
            _animator.SetTrigger(_turnTrigger);
        }

        public bool IsTurnFinished()
        {
            if (!IsReady || _currentTurnState == 0)
                return false;

            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);

            if (state.fullPathHash == _currentTurnState)
                _turnStateSeen = true;

            // Turn을 실제로 재생하고 복귀 전환까지 마쳐야 종료한다.
            if (!_turnStateSeen || _animator.IsInTransition(0))
                return false;

            // Turn에서 해당 이동 상태로 돌아온 뒤 종료한다.
            return state.fullPathHash == _walk ||
                   state.fullPathHash == _jog ||
                   state.fullPathHash == _sprint;
        }

        public void SetTurnRootMotion(bool enabled)
        {
            if (_animator == null || _useTurnRootMotion == enabled)
                return;

            _useTurnRootMotion = enabled;
            _animator.applyRootMotion = enabled;

            if (!enabled)
            {
                _currentTurnState = 0;
                _turnStateSeen = false;
                // applyRootMotion 변경으로 Animator가 재초기화될 수 있다.
                _currentMoveState = 0;
            }
        }

        /*private void OnAnimatorMove()
        {
            if (!_useTurnRootMotion || _animator == null || _characterRoot == null)
                return;

            // Turn 중 Animator의 이동과 회전을 Player 루트에 적용한다.
            _characterRoot.position += _animator.deltaPosition;
            _characterRoot.rotation *= _animator.deltaRotation;
        }*/
    }
}
