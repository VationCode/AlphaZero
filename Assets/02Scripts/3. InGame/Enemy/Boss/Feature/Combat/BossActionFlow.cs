using System;
using System.Collections.Generic;
using UnityEngine;

namespace alpha.ingame.enemy.boss
{
    public enum EBossActionState
    {
        Idle,
        Chasing,
        Repositioning,
        Attacking,
        AttackRecovery,
        Groggy
    }

    /// <summary>전투 중 행동 상태의 전환만 판단한다.</summary>
    [DisallowMultipleComponent]
    public sealed class BossActionFlow : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _attackRecoveryDuration = 0.75f;

        private BossEncounterFlow _encounter;
        private BossIdleState _idle;
        private BossChasingState _chasing;
        private BossAttackingState _attacking;
        private BossActionModule _actions;
        private BossLocomotionModule _locomotion;
        private float _recoveryEndsAt;
        private Transform _targetingTarget;
        private Vector3 _targetingPosition;
        private bool _targetingLocked;
        private Vector3 _attack3LandingTarget;
        private bool _attack3JumpStarted;
        private bool _attack5ButtStarted;
        private Vector3 _initialBossPosition;
        private Vector3 _initialBossForward;
        private EAttack7Phase _attack7Phase;
        private readonly Dictionary<string, ChaseFailure> _chaseFailures = new();

        private enum EAttack7Phase { None, Jump, FaceInitialForward, Attack }

        private readonly struct ChaseFailure
        {
            public readonly Transform Target;
            public readonly float Distance;
            public readonly float RetryImprovement;

            public ChaseFailure(Transform target, float distance, float retryImprovement)
            {
                Target = target;
                Distance = distance;
                RetryImprovement = retryImprovement;
            }
        }

        public EBossActionState CurrentState { get; private set; } = EBossActionState.Idle;
        public string CurrentAttackId { get; private set; }
        public event Action<EBossActionState> StateChanged;
        public event Action AttackStageChanged;

        public void Bind(BossEncounterFlow encounter, BossIdleState idle,
            BossChasingState chasing, BossAttackingState attacking,
            BossActionModule actionModule,
            BossChasingModule chasingModule, BossLocomotionModule locomotion,
            Vector3 initialBossPosition, Vector3 initialBossForward)
        {
            if (_encounter != null)
                _encounter.StateChanged -= HandleEncounterStateChanged;

            _encounter = encounter;
            _idle = idle;
            _chasing = chasing;
            _attacking = attacking;
            _actions = actionModule;
            _locomotion = locomotion;
            _initialBossPosition = initialBossPosition;
            _initialBossForward = Vector3.ProjectOnPlane(initialBossForward, Vector3.up).normalized;
            _idle?.Bind(this, actionModule);
            _chasing?.Bind(this, chasingModule);
            if (_encounter == null) return;

            _encounter.StateChanged += HandleEncounterStateChanged;
            HandleEncounterStateChanged(_encounter.CurrentState);
        }

        private void Update()
        {
            if (_encounter == null ||
                _encounter.CurrentState != EBossEncounterState.Combat) return;

            if (CurrentState == EBossActionState.AttackRecovery)
            {
                if (Time.time >= _recoveryEndsAt)
                    SetState(EBossActionState.Idle);
                return;
            }

            if (CurrentState == EBossActionState.Idle && _idle != null)
                _idle.Tick();
        }

        private void FixedUpdate()
        {
            if (_encounter == null ||
                _encounter.CurrentState != EBossEncounterState.Combat) return;

            if (CurrentState == EBossActionState.Chasing && _chasing != null)
                _chasing.FixedTick();
            else if (CurrentState == EBossActionState.Attacking &&
                _attack7Phase == EAttack7Phase.FaceInitialForward &&
                _locomotion != null && _locomotion.FaceForward(_initialBossForward))
            {
                _attack7Phase = EAttack7Phase.Attack;
                CurrentAttackId = "Attack7";
                AttackStageChanged?.Invoke();
            }
        }

        public bool TryBeginChasing(Transform target, BossAttackCastConditions attack)
        {
            if (_encounter == null ||
                _encounter.CurrentState != EBossEncounterState.Combat ||
                CurrentState != EBossActionState.Idle ||
                attack == null || !attack.IsEnabled ||
                _chasing == null ||
                !_chasing.Enter(target, attack)) return false;

            SetState(EBossActionState.Chasing);
            return true;
        }

        public bool TryBeginAttack(Transform target, BossAttackCastConditions attack)
        {
            if (_encounter == null || _encounter.CurrentState != EBossEncounterState.Combat ||
                (CurrentState != EBossActionState.Idle && CurrentState != EBossActionState.Chasing) ||
                attack == null || !attack.IsEnabled || (attack.AttackId != "Attack1" &&
                    attack.AttackId != "Attack2" && attack.AttackId != "Attack3" &&
                    attack.AttackId != "Attack4" && attack.AttackId != "Attack5" &&
                    attack.AttackId != "Attack6" && attack.AttackId != "Attack7") ||
                _attacking == null || _actions == null || _locomotion == null ||
                !_locomotion.CanMove || _locomotion.IsJumping || _locomotion.IsButting ||
                target == null || !_locomotion.IsFacing(target.position) ||
                !_attacking.CanStart(attack, _actions.Owner, target)) return false;

            if (!_attacking.Enter(attack, _actions.Owner, target)) return false;
            // TargetingStart를 사용하는 공격은 이벤트 시점의 대상 위치를 확정한다.
            _targetingTarget = target;
            _targetingLocked = false;
            // 단독 Attack3은 타깃 위치, Attack7의 선행 점프는 최초 위치를 착지점으로 사용한다.
            _attack7Phase = attack.AttackId == "Attack7" ? EAttack7Phase.Jump : EAttack7Phase.None;
            if (attack.AttackId == "Attack3" || _attack7Phase == EAttack7Phase.Jump)
                _attack3LandingTarget = _attack7Phase == EAttack7Phase.Jump
                    ? _initialBossPosition : target.position;
            CurrentAttackId = _attack7Phase == EAttack7Phase.Jump ? "Attack3" : attack.AttackId;
            SetState(EBossActionState.Attacking);
            return true;
        }

        public void CaptureTargetingPosition()
        {
            if (CurrentState != EBossActionState.Attacking || _targetingLocked ||
                _targetingTarget == null || !_targetingTarget.gameObject.activeInHierarchy) return;

            _targetingPosition = _targetingTarget.position;
            _targetingLocked = true;
        }

        public bool TryGetTargetingPosition(out Vector3 position)
        {
            position = _targetingPosition;
            return CurrentState == EBossActionState.Attacking && _targetingLocked;
        }

        public bool TryGetAttack7SpawnPositions(float minSpacing, List<Vector3> positions)
        {
            if (CurrentState != EBossActionState.Attacking ||
                CurrentAttackId != "Attack7" || !_targetingLocked ||
                _actions == null || _targetingTarget == null ||
                !_targetingTarget.gameObject.activeInHierarchy) return false;

            CharacterController player = _targetingTarget.GetComponent<CharacterController>();
            if (player == null || !player.enabled) return false;
            _actions.GetAttack7SpawnPositions(_targetingPosition, minSpacing, player, positions);
            return positions.Count > 0;
        }

        public bool IsAttackReady(BossAttackCastConditions attack) =>
            attack != null && attack.IsEnabled &&
            (attack.AttackId == "Attack1" || attack.AttackId == "Attack2" ||
                attack.AttackId == "Attack3" || attack.AttackId == "Attack4" ||
                attack.AttackId == "Attack5" || attack.AttackId == "Attack6" ||
                attack.AttackId == "Attack7") &&
            _attacking != null && _attacking.IsReady(attack);

        public void BeginAttack5Butt()
        {
            if (CurrentState != EBossActionState.Attacking ||
                CurrentAttackId != "Attack5" || _attack5ButtStarted ||
                _locomotion == null || _actions == null) return;
            _attack5ButtStarted = _locomotion.BeginButtMotion(_actions.Attack5ButtDistance);
        }

        public void UpdateAttack5Butt(float progress)
        {
            if (CurrentState == EBossActionState.Attacking &&
                CurrentAttackId == "Attack5" && _attack5ButtStarted)
                _locomotion?.SetButtProgress(progress);
        }

        public void EndAttack5Butt()
        {
            if (CurrentState != EBossActionState.Attacking ||
                CurrentAttackId != "Attack5" || !_attack5ButtStarted) return;
            _locomotion?.EndButtMotion();
            _attack5ButtStarted = false;
        }

        // Attack3 클립의 JumpStart / JumpEnd 이벤트가 공중 이동을 구획한다.
        public void BeginAttack3Jump()
        {
            if (CurrentState != EBossActionState.Attacking ||
                CurrentAttackId != "Attack3" || _attack3JumpStarted ||
                _locomotion == null) return;

            _attack3JumpStarted = _locomotion.BeginJumpTo(_attack3LandingTarget);
        }

        public void UpdateAttack3Jump(float progress)
        {
            if (CurrentState == EBossActionState.Attacking &&
                CurrentAttackId == "Attack3" && _attack3JumpStarted)
                _locomotion?.SetJumpProgress(progress);
        }

        public void EndAttack3Jump()
        {
            if (CurrentState != EBossActionState.Attacking ||
                CurrentAttackId != "Attack3" || !_attack3JumpStarted) return;
            _locomotion?.CompleteJump();
            _attack3JumpStarted = false;
        }

        // View의 재생 완료 신호를 받아 상태만 전환한다.
        public void CompleteAttackAnimation()
        {
            if (CurrentState != EBossActionState.Attacking || _attacking == null) return;

            // Attack7은 Attack3 착지 후 처음 배치된 정면으로 돌아선 다음 시작한다.
            if (_attack7Phase == EAttack7Phase.Jump)
            {
                if (_attack3JumpStarted) _locomotion?.CompleteJump();
                _attack3JumpStarted = false;
                _attack7Phase = EAttack7Phase.FaceInitialForward;
                CurrentAttackId = null;
                AttackStageChanged?.Invoke();
                return;
            }
            if (_attack7Phase == EAttack7Phase.FaceInitialForward ||
                !_attacking.Complete()) return;

            if (_attack3JumpStarted) _locomotion?.CompleteJump();
            if (_attack5ButtStarted) _locomotion?.EndButtMotion();
            _recoveryEndsAt = Time.time + _attackRecoveryDuration;
            SetState(EBossActionState.AttackRecovery);
        }

        public void RecordChaseFailure(BossAttackCastConditions attack, Transform target,
            float distance, float retryImprovement)
        {
            if (attack == null || string.IsNullOrEmpty(attack.AttackId)) return;
            _chaseFailures[attack.AttackId] = new ChaseFailure(target, distance,
                Mathf.Max(0.01f, retryImprovement));
        }

        public bool IsChaseBlocked(BossAttackCastConditions attack, Transform target,
            float distance)
        {
            if (attack == null || string.IsNullOrEmpty(attack.AttackId) ||
                !_chaseFailures.TryGetValue(attack.AttackId, out ChaseFailure failure))
                return false;

            // 상황이 바뀌면 실패한 공격을 다시 후보로 허용한다.
            if (failure.Target != target || distance <= attack.CastDistance ||
                distance <= failure.Distance - failure.RetryImprovement)
            {
                _chaseFailures.Remove(attack.AttackId);
                return false;
            }
            return true;
        }

        public bool TryChangeState(EBossActionState next)
        {
            if (_encounter == null ||
                _encounter.CurrentState != EBossEncounterState.Combat ||
                next == EBossActionState.Chasing ||
                next == EBossActionState.Repositioning ||
                next == EBossActionState.Attacking ||
                next == EBossActionState.AttackRecovery ||
                next == CurrentState || !CanTransition(CurrentState, next)) return false;

            SetState(next);
            return true;
        }

        private void HandleEncounterStateChanged(EBossEncounterState encounterState)
        {
            // 인트로·페이즈 전환·사망 중에는 전투 행동을 진행하지 않는다.
            if (encounterState != EBossEncounterState.Combat)
            {
                _chaseFailures.Clear();
                SetState(EBossActionState.Idle);
            }
        }

        private void SetState(EBossActionState next)
        {
            if (CurrentState == next) return;
            if (CurrentState == EBossActionState.Chasing)
                _chasing?.Exit();
            if (CurrentState == EBossActionState.Attacking)
            {
                if (_attack3JumpStarted) _locomotion?.Stop();
                if (_attack5ButtStarted) _locomotion?.Stop();
                _targetingTarget = null;
                _targetingLocked = false;
                _attack3JumpStarted = false;
                _attack5ButtStarted = false;
                _attack7Phase = EAttack7Phase.None;
                _attacking?.Exit();
                CurrentAttackId = null;
            }
            if (CurrentState == EBossActionState.AttackRecovery)
            {
                _recoveryEndsAt = 0f;
                _locomotion?.UnlockRecoveryRotation();
            }
            CurrentState = next;
            if (next == EBossActionState.AttackRecovery)
                _locomotion?.LockRecoveryRotation();
            StateChanged?.Invoke(next);
        }

        private static bool CanTransition(EBossActionState current, EBossActionState next)
        {
            if (next == EBossActionState.Groggy) return true;

            return current switch
            {
                EBossActionState.Idle => next == EBossActionState.Chasing ||
                    next == EBossActionState.Repositioning || next == EBossActionState.Attacking,
                EBossActionState.Chasing => next == EBossActionState.Idle ||
                    next == EBossActionState.Repositioning || next == EBossActionState.Attacking,
                EBossActionState.Repositioning => next == EBossActionState.Idle ||
                    next == EBossActionState.Chasing || next == EBossActionState.Attacking,
                EBossActionState.Attacking => next == EBossActionState.AttackRecovery,
                EBossActionState.AttackRecovery => next == EBossActionState.Idle ||
                    next == EBossActionState.Chasing || next == EBossActionState.Repositioning,
                EBossActionState.Groggy => next == EBossActionState.Idle ||
                    next == EBossActionState.Repositioning,
                _ => false
            };
        }

        private void OnDestroy()
        {
            if (_encounter != null)
                _encounter.StateChanged -= HandleEncounterStateChanged;
            _idle?.Bind(null, null);
            _chasing?.Bind(null, null);
            _attacking?.Exit();
            if (_attack3JumpStarted) _locomotion?.Stop();
            if (_attack5ButtStarted) _locomotion?.Stop();
            _locomotion?.UnlockRecoveryRotation();
        }

        private void OnDisable()
        {
            _targetingTarget = null;
            _targetingLocked = false;
            if (CurrentState == EBossActionState.Chasing)
            {
                _chasing?.Exit();
                CurrentState = EBossActionState.Idle;
            }
            else if (CurrentState == EBossActionState.Attacking)
            {
                if (_attack3JumpStarted) _locomotion?.Stop();
                if (_attack5ButtStarted) _locomotion?.Stop();
                _attack3JumpStarted = false;
                _attack5ButtStarted = false;
                _attack7Phase = EAttack7Phase.None;
                _attacking?.Exit();
                CurrentAttackId = null;
                CurrentState = EBossActionState.Idle;
            }
            else if (CurrentState == EBossActionState.AttackRecovery)
            {
                _recoveryEndsAt = 0f;
                _locomotion?.UnlockRecoveryRotation();
                CurrentState = EBossActionState.Idle;
            }
        }

        private void OnValidate()
        {
            _attackRecoveryDuration = Mathf.Max(0f, _attackRecoveryDuration);
        }
    }
}
