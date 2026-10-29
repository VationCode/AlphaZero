using System;
using System.Collections.Generic;
using UnityEngine;

namespace alpha.ingame.enemy.boss
{
    /// <summary>보스 행동 상태를 Animator의 이동·대기 표현으로 연결한다.</summary>
    [DisallowMultipleComponent]
    public sealed class BossAnimationView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private string _idleState = "Idle";
        [SerializeField] private string _chaseState = "Walk_F";
        [SerializeField] private string _attack1State = "Attack1";
        [SerializeField] private string _attack2State = "Attack2";
        [SerializeField] private string _attack3State = "Attack3";
        [SerializeField] private string _attack4State = "Attack4";
        [SerializeField] private string _attack5State = "Attack5";
        [SerializeField] private string _attack6State = "Attack6";
        [SerializeField] private string _attack7State = "Attack7";
        [SerializeField] private BossAttackEffectView _attackEffects;

        private BossActionFlow _flow;
        private bool _awaitingAttackEnd;
        private string _currentAttackState;
        private Transform _attack5MotionRoot;
        private Transform _attack5PivotRoot;
        private float _attack5RootRestZ;
        private float _attack5PivotRestZ;
        private bool _attack5Compensating;
        private bool _attack5ButtActive;
        private float _attack5ButtStartNormalizedTime;
        private float _attack5ButtEndNormalizedTime;
        private bool _attack3JumpActive;
        private float _attack3JumpStartNormalizedTime;
        private float _attack3JumpEndNormalizedTime;
        private readonly List<Vector3> _attack7SpawnPositions = new();

        public bool IsReady => _animator != null && _animator.runtimeAnimatorController != null;
        public event Action AttackAnimationFinished;
        public event Action Attack3JumpStarted;
        public event Action<float> Attack3JumpProgressChanged;
        public event Action Attack3JumpEnded;
        public event Action Attack5ButtStarted;
        public event Action<float> Attack5ButtProgressChanged;
        public event Action Attack5ButtEnded;

        private void Awake()
        {
            if (_attackEffects == null)
                _attackEffects = transform.parent?.GetComponentInChildren<BossAttackEffectView>(true);
        }

        public void NotifyEffect(string eventKey, bool active)
        {
            if (_flow == null || string.IsNullOrEmpty(_flow.CurrentAttackId)) return;
            if (active && _flow.CurrentState != EBossActionState.Attacking) return;
            _attackEffects?.SetEffect(_flow.CurrentAttackId, eventKey, active);
        }

        public void NotifyTargetingStart() => _flow?.CaptureTargetingPosition();

        public void NotifyEffectSpawn(string eventKey)
        {
            if (_flow == null || _flow.CurrentState != EBossActionState.Attacking ||
                string.IsNullOrEmpty(_flow.CurrentAttackId)) return;

            string attackId = _flow.CurrentAttackId;
            if (_attackEffects != null && _attackEffects.IsRandomTargetArea(attackId, eventKey))
            {
                if (attackId == "Attack7" &&
                    _attackEffects.TryGetSpawnSpacing(attackId, eventKey, out float spacing) &&
                    _flow.TryGetAttack7SpawnPositions(spacing, _attack7SpawnPositions))
                    _attackEffects.SpawnEffectAtPositions(attackId, eventKey,
                        _attack7SpawnPositions);
                return;
            }

            bool hasTargetPosition = _flow.TryGetTargetingPosition(out Vector3 position);
            _attackEffects?.SpawnEffect(attackId, eventKey,
                hasTargetPosition, position);
        }

        // Animator 자식 오브젝트의 AnimationEvent를 전투 Flow에 전달한다.
        public void NotifyJumpStart(AnimationEvent animationEvent)
        {
            if (_flow?.CurrentAttackId != "Attack3" ||
                !TryGetEventInterval(animationEvent,
                    nameof(BossAttackAnimationEventRelay.JumpEnd),
                    out _attack3JumpStartNormalizedTime,
                    out _attack3JumpEndNormalizedTime)) return;

            _attack3JumpActive = true;
            Attack3JumpStarted?.Invoke();
        }

        public void NotifyJumpEnd()
        {
            if (!_attack3JumpActive) return;
            _attack3JumpActive = false;
            Attack3JumpEnded?.Invoke();
        }

        public void NotifyButtStart(AnimationEvent animationEvent)
        {
            if (!_attack5Compensating || _flow?.CurrentAttackId != "Attack5") return;
            if (!TryGetEventInterval(animationEvent,
                    nameof(BossAttackAnimationEventRelay.ButtEnd),
                    out _attack5ButtStartNormalizedTime,
                    out _attack5ButtEndNormalizedTime)) return;
            _attack5ButtActive = true;
            Attack5ButtStarted?.Invoke();
        }

        private bool TryGetEventInterval(AnimationEvent animationEvent, string endEventName,
            out float startNormalizedTime, out float endNormalizedTime)
        {
            startNormalizedTime = 0f;
            endNormalizedTime = 0f;
            AnimationClip clip = animationEvent?.animatorClipInfo.clip;
            if (clip == null || clip.length <= 0f)
            {
                Debug.LogError($"{name}: 공격 이벤트의 애니메이션 클립을 읽지 못했습니다.", this);
                return false;
            }

            float startTime = animationEvent.time;
            float endTime = float.PositiveInfinity;
            foreach (AnimationEvent clipEvent in clip.events)
            {
                if (clipEvent.functionName == endEventName && clipEvent.time > startTime)
                    endTime = Mathf.Min(endTime, clipEvent.time);
            }
            if (float.IsPositiveInfinity(endTime))
            {
                Debug.LogError($"{name}: {clip.name} 클립에 {endEventName} 이벤트가 없습니다.", this);
                return false;
            }

            startNormalizedTime = startTime / clip.length;
            endNormalizedTime = endTime / clip.length;
            return true;
        }

        public void NotifyButtEnd()
        {
            if (!_attack5ButtActive) return;
            _attack5ButtActive = false;
            Attack5ButtEnded?.Invoke();
        }

        public void Bind(BossActionFlow flow)
        {
            if (_flow != null) _flow.StateChanged -= HandleStateChanged;
            if (_flow != null) _flow.AttackStageChanged -= HandleAttackStageChanged;
            _flow = flow;
            if (isActiveAndEnabled && _flow != null)
            {
                _flow.StateChanged += HandleStateChanged;
                _flow.AttackStageChanged += HandleAttackStageChanged;
            }
        }

        private void Start()
        {
            if (_flow != null) HandleStateChanged(_flow.CurrentState);
        }

        private void OnEnable()
        {
            if (_flow == null) return;
            _flow.StateChanged -= HandleStateChanged;
            _flow.AttackStageChanged -= HandleAttackStageChanged;
            _flow.StateChanged += HandleStateChanged;
            _flow.AttackStageChanged += HandleAttackStageChanged;
            HandleStateChanged(_flow.CurrentState);
        }

        private void OnDisable()
        {
            _attackEffects?.StopAll();
            _awaitingAttackEnd = false;
            _currentAttackState = null;
            _attack5Compensating = false;
            _attack5ButtActive = false;
            _attack3JumpActive = false;
            if (_flow == null) return;
            _flow.StateChanged -= HandleStateChanged;
            _flow.AttackStageChanged -= HandleAttackStageChanged;
        }

        private void LateUpdate()
        {
            UpdateAttack3JumpAnimation();
            UpdateAttack5ButtAnimation();
            if (!_awaitingAttackEnd || !IsReady || _animator.IsInTransition(0)) return;

            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            if (state.shortNameHash != Animator.StringToHash(_currentAttackState) ||
                state.normalizedTime < 1f) return;

            _awaitingAttackEnd = false;
            _currentAttackState = null;
            AttackAnimationFinished?.Invoke();
        }

        private void UpdateAttack3JumpAnimation()
        {
            if (!_attack3JumpActive) return;
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            if (state.shortNameHash != Animator.StringToHash(_attack3State)) return;

            float progress = Mathf.InverseLerp(_attack3JumpStartNormalizedTime,
                _attack3JumpEndNormalizedTime, state.normalizedTime);
            Attack3JumpProgressChanged?.Invoke(progress);
        }

        private void UpdateAttack5ButtAnimation()
        {
            if (!_attack5Compensating || _attack5MotionRoot == null) return;

            // 이벤트 사이의 클립 진행률로 이동하고, 자식 root의 중복 이동은 지운다.
            if (_attack5ButtActive)
            {
                AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
                if (state.shortNameHash == Animator.StringToHash(_attack5State))
                {
                    float progress = Mathf.InverseLerp(_attack5ButtStartNormalizedTime,
                        _attack5ButtEndNormalizedTime, state.normalizedTime);
                    Attack5ButtProgressChanged?.Invoke(progress);
                }
            }

            ResetRootZ(_attack5MotionRoot, _attack5RootRestZ);
            if (_attack5PivotRoot != null)
                ResetRootZ(_attack5PivotRoot, _attack5PivotRestZ);

            // Attack5에서 Idle로 블렌딩하는 동안에도 모델이 뒤로 미끄러지지 않게 한다.
            if (_flow?.CurrentAttackId != "Attack5" &&
                !_animator.IsInTransition(0) &&
                _animator.GetCurrentAnimatorStateInfo(0).shortNameHash !=
                    Animator.StringToHash(_attack5State))
                _attack5Compensating = false;
        }

        private static void ResetRootZ(Transform root, float restZ)
        {
            Vector3 position = root.localPosition;
            position.z = restZ;
            root.localPosition = position;
        }

        private void BeginAttack5Compensation()
        {
            if (_attack5MotionRoot == null)
            {
                _attack5MotionRoot = _animator.transform.Find("root");
                if (_attack5MotionRoot == null)
                {
                    Debug.LogError($"{name}: Attack5의 root Transform을 찾지 못했습니다.", this);
                    return;
                }
                _attack5RootRestZ = _attack5MotionRoot.localPosition.z;
                _attack5PivotRoot = _animator.transform.Find("Pivot_for_root");
                if (_attack5PivotRoot != null)
                    _attack5PivotRestZ = _attack5PivotRoot.localPosition.z;
            }
            _attack5Compensating = true;
            _attack5ButtActive = false;
        }

        private void HandleAttackStageChanged() => HandleStateChanged(EBossActionState.Attacking);

        private void HandleStateChanged(EBossActionState state)
        {
            if (!IsReady || !_animator.isActiveAndEnabled) return;
            string attackId = state == EBossActionState.Attacking
                ? _flow?.CurrentAttackId : null;
            if (state != EBossActionState.Attacking) _attackEffects?.StopAll();
            _attack3JumpActive = false;
            if (attackId == "Attack5") BeginAttack5Compensation();
            _currentAttackState = attackId switch
            {
                "Attack1" => _attack1State,
                "Attack2" => _attack2State,
                "Attack3" => _attack3State,
                "Attack4" => _attack4State,
                "Attack5" => _attack5State,
                "Attack6" => _attack6State,
                "Attack7" => _attack7State,
                _ => null
            };
            _awaitingAttackEnd = !string.IsNullOrEmpty(_currentAttackState);
            string animationState = state switch
            {
                EBossActionState.Idle => _idleState,
                EBossActionState.AttackRecovery => _idleState,
                EBossActionState.Chasing => _chaseState,
                EBossActionState.Attacking => _currentAttackState ?? _chaseState,
                _ => null
            };
            if (string.IsNullOrEmpty(animationState)) return;
            if (state == EBossActionState.Idle)
            {
                if (!_animator.IsInTransition(0) &&
                    _animator.GetCurrentAnimatorStateInfo(0).shortNameHash ==
                    Animator.StringToHash(_idleState)) return;
                _animator.Play(animationState, 0, 0f);
            }
            else
                _animator.CrossFadeInFixedTime(animationState,
                    state == EBossActionState.Chasing ? 0.05f : 0.15f, 0, 0f);
        }
    }
}
