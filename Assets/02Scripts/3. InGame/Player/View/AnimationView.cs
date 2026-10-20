using System;
using UnityEngine;

namespace alpha.ingame.player
{
    public class AnimationView : MonoBehaviour
    {
        [Serializable]
        private struct AreteAnimatorController
        {
            [SerializeField] private EAreteType _areteType;
            [SerializeField] private RuntimeAnimatorController _baseController;

            public EAreteType AreteType => _areteType;
            public RuntimeAnimatorController BaseController => _baseController;
        }

        [Header("Ref")]
        [SerializeField] private Transform _characterRoot;
        [SerializeField] private Animator _animator;
        [SerializeField] private AreteAnimatorController[] _areteControllers;

        [Header("Blend Time")]
        [SerializeField] private float _moveBlendTime = 0.12f;

        [Header("Jump To Fall")]
        [SerializeField, Range(0f, 1f)] private float _jumpFallStart = 0.4f;
        [SerializeField, Range(0f, 1f)] private float _doubleJumpFallStart = 0.65f;

        [Header("Fall Crossfade")]
        [SerializeField, Min(0f)] private float _fallBlendTime = 0.1f;

        // Animator 상태
        private readonly int _walk = Animator.StringToHash("Base Layer.Walk");
        private readonly int _sprintTree = Animator.StringToHash("Base Layer.Sprint Tree");
        private readonly int _boostTree = Animator.StringToHash("Base Layer.Boost Tree");
        private readonly int _boostCombatTree = Animator.StringToHash("Base Layer.Boost Combat Tree");
        private readonly int _combatMoveTree = Animator.StringToHash("Base Layer.CombatMove Tree");

        private readonly int _jump = Animator.StringToHash("Base Layer.Jump");
        private readonly int _doubleJump = Animator.StringToHash("Base Layer.Double Jump");
        private readonly int _fall = Animator.StringToHash("Base Layer.Fall");
        private readonly int _land = Animator.StringToHash("Base Layer.Land");

        // Blend Tree 파라미터
        private readonly int _inputX = Animator.StringToHash("InputX");
        private readonly int _inputY = Animator.StringToHash("InputY");
        private readonly int _moveMagnitude = Animator.StringToHash("MoveMagnitude");

        private int _currentMoveState;

        private void Start()
        {
            if (_characterRoot == null)
            {
                _characterRoot = GetComponentInParent<CharacterController>().transform;
            }
        }

        public void Initialize(Animator p_animator, EAreteType p_areteType)
        {
            if (p_animator == null)
            {
                Debug.LogWarning($"{nameof(AnimationView)}: {p_areteType} 모델에 Animator가 없습니다.", this);
                return;
            }

            // 생성된 모델의 Animator에 해당 아레테 컨트롤러를 적용한다.
            _animator = p_animator;
            HandlePlayerSetup(p_areteType);
        }

        public void HandlePlayerSetup(EAreteType p_type)
        {
            if (_animator == null || _areteControllers == null)
                return;

            // 아레테 타입으로 기본 컨트롤러를 찾아 생성된 모델의 Animator에 적용한다.
            foreach (AreteAnimatorController entry in _areteControllers)
            {
                if (entry.AreteType != p_type)
                    continue;

                if (entry.BaseController == null)
                    break;

                _animator.runtimeAnimatorController = entry.BaseController;
                _currentMoveState = 0;
                return;
            }

            Debug.LogWarning($"{nameof(AnimationView)}: {p_type} 기본 Animator Controller가 없습니다.", this);
        }

        public void MoveAnim(Vector3 p_moveDirection, EMoveType p_moveType, float p_inputStrength)
        {
            if (_animator == null || !_animator.isActiveAndEnabled ||
                _animator.runtimeAnimatorController == null)
                return;

            float strength = Mathf.Clamp01(p_inputStrength);
            // 걷기 전환은 이동 중에만 적용하고 전투 이동을 우선한다.
            int nextState = p_moveType switch
            {
                EMoveType.Combat => _combatMoveTree,
                EMoveType.Boost => _boostTree,
                EMoveType.BoostCombat => _boostCombatTree,
                EMoveType.Walk when strength > 0f => _walk,
                _ => _sprintTree
            };

            // 월드 방향을 캐릭터 기준으로 바꾸고 입력 세기를 반영한다.
            Vector3 localDirection =
                _characterRoot != null ? _characterRoot.InverseTransformDirection(p_moveDirection) : p_moveDirection;
            localDirection.y = 0f;
            localDirection.Normalize();

            _animator.SetFloat(_inputX, localDirection.x * strength, 0.1f, Time.deltaTime);
            _animator.SetFloat(_inputY, localDirection.z * strength, 0.1f, Time.deltaTime);
            _animator.SetFloat(_moveMagnitude, strength, 0.1f, Time.deltaTime);

            if (_currentMoveState == nextState)
                return;

            _currentMoveState = nextState;
            _animator.CrossFadeInFixedTime(nextState, _moveBlendTime, 0, 0f);
        }

        public void JumpAnim(int p_jumpCount)
        {
            if (_animator == null || !_animator.isActiveAndEnabled) return;

            int state = p_jumpCount == 2 ? _doubleJump : _jump;
            if (!_animator.HasState(0, state)) return;

            _currentMoveState = 0;
            _animator.CrossFadeInFixedTime(state, 0.08f, 0, 0f); // 점프 시작부터 재생
        }

        public bool CanEnterFallFromJump()
        {
            if (_animator == null || !_animator.isActiveAndEnabled) return true;
            if (_animator.IsInTransition(0)) return false;

            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            if (state.fullPathHash == _jump)
                return state.normalizedTime >= _jumpFallStart;
            if (state.fullPathHash == _doubleJump)
                return state.normalizedTime >= _doubleJumpFallStart;

            return true;
        }


        public void FallAnim()
        {
            if (_animator == null || !_animator.isActiveAndEnabled ||
                !_animator.HasState(0, _fall))
                return;

            _currentMoveState = 0;

            _animator.CrossFadeInFixedTime(_fall, _fallBlendTime, 0, 0f); // 별도 Fall 클립을 처음부터 짧게 섞는다.
        }

        public void LandAnim()
        {
            if (_animator == null || !_animator.isActiveAndEnabled ||
        !_animator.HasState(0, _land))
                return;

            _currentMoveState = 0;
            _animator.CrossFadeInFixedTime(_land, 0.174f, 0, 0.1416163f);
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
