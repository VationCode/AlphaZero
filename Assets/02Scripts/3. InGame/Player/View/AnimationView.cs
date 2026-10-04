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

        private readonly int _idleWalkQuickTurn = Animator.StringToHash("Base Layer.Idle Walk Quick Turn");
        private readonly int _idleJogQuickTurn = Animator.StringToHash("Base Layer.Idle Jog Quick Turn");
        private readonly int _idleSprintQuickTurn = Animator.StringToHash("Base Layer.Idle Sprint Quick Turn");
        private readonly int _walkQuickTurn = Animator.StringToHash("Base Layer.Walk Quick Turn");
        private readonly int _jogQuickTurn = Animator.StringToHash("Base Layer.Jog Quick Turn");
        private readonly int _sprintQuickTurn = Animator.StringToHash("Base Layer.Sprint Quick Turn");

        // Blend Tree 파라미터
        private readonly int _inputX = Animator.StringToHash("InputX");
        private readonly int _inputY = Animator.StringToHash("InputY");
        private readonly int _moveSpeed = Animator.StringToHash("MoveSpeed");

        private int _currentMoveState;
        private int _currentTurnState;
        private float _turnBlendTime;
        private bool _useTurnRootMotion;


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
            _useTurnRootMotion = false;
        }

        public void MoveAnim(Vector3 p_moveDirection, EMoveType p_moveType, float p_currentSpeed)
        {
            if (_animator == null || !_animator.isActiveAndEnabled ||
                _animator.runtimeAnimatorController == null || _useTurnRootMotion)
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

            // 턴 직후 첫 호출은 같은 Move Tree라도 전환한다.
            if (_currentMoveState == nextState && _currentTurnState == 0)
                return;

            _currentMoveState = nextState;
            _currentTurnState = 0;
            _animator.CrossFadeInFixedTime(nextState, _moveBlendTime, 0, 0f);
        }

        public void TurnAnim(EMoveType p_preType, EMoveType p_nextType)
        {
            _currentTurnState = GetTurnState(p_preType, p_nextType);
            
            // Walk → Quick Turn에만 긴 진입 블렌드를 적용한다.(해당 클립들 트랜지션 저 값들로 설정해야 부드럽게 연결)
            _turnBlendTime = p_preType == EMoveType.Walk ? 0.25f : 0.05f;

            SetTurnRootMotion(true);

            _animator.CrossFadeInFixedTime(_currentTurnState, _turnBlendTime, 0, 0f);
        }

        private int GetTurnState(EMoveType p_previousType, EMoveType p_nextType)
        {
            // Idle에서 시작하면 이번 이동 타입으로 턴을 고른다.
            if (p_previousType == EMoveType.Idle)
            {
                return p_nextType switch
                {
                    EMoveType.Walk => _idleWalkQuickTurn,
                    EMoveType.Jog => _idleJogQuickTurn,
                    EMoveType.Sprint => _idleSprintQuickTurn,
                    _ => 0
                };
            }

            // 이동 중이면 이전 이동 타입으로 턴을 고른다.
            return p_previousType switch
            {
                EMoveType.Walk => _walkQuickTurn,
                EMoveType.Jog => _jogQuickTurn,
                EMoveType.Sprint => _sprintQuickTurn,
                _ => 0
            };
        }

        // 턴 종료 확인
        public bool ShouldReturnFromTurn()
        {
            if (_animator == null || _currentTurnState == 0 || _animator.IsInTransition(0))
                return false;

            AnimatorStateInfo turn = _animator.GetCurrentAnimatorStateInfo(0);

            if (turn.fullPathHash != _currentTurnState)
                return false;

            // 복귀 블렌드 시간만큼 턴이 남으면 이동을 재개한다.
            float remainingTime = (1f - turn.normalizedTime) * turn.length;

            if (remainingTime > _moveBlendTime)
                return false;

                SetTurnRootMotion(false);

            return true;
        }

        public void SetTurnRootMotion(bool enabled)
        {
            if (_animator == null || _useTurnRootMotion == enabled)
                return;

            _useTurnRootMotion = enabled;
        }

        private void OnAnimatorMove()
        {
            if (!_useTurnRootMotion || _animator == null || _characterRoot == null)
                return;

            // Turn 중 Animator의 이동과 회전을 Player 루트에 적용한다.
            _characterRoot.position += _animator.deltaPosition;
            _characterRoot.rotation *= _animator.deltaRotation;
        }
    }
}
