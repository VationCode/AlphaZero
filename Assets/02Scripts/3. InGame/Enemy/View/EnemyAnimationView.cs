using UnityEngine;

namespace alpha.ingame.enemy
{
    /// <summary>순찰과 추적의 이동 및 대기 단계를 애니메이션으로 표현한다.</summary>
    public sealed class EnemyAnimationView : MonoBehaviour
    {
        private static readonly int IdleState = Animator.StringToHash("Base Layer.Idle");
        private static readonly int Idle2State = Animator.StringToHash("Base Layer.Idle2");
        private static readonly int WalkState = Animator.StringToHash("Base Layer.Walk");
        private static readonly int RunState = Animator.StringToHash("Base Layer.Run");

        [SerializeField, Min(0f)] private float _walkSpeedThreshold = 0.05f;
        [SerializeField, Min(0f)] private float _transitionDuration = 0.15f;

        private EnemyCore _enemy;
        private Animator _animator;
        private int _currentState;
        private bool _hasIdle2State;
        private bool _hasRunState;

        private void Awake()
        {
            _enemy = GetComponentInParent<EnemyCore>(true);
            _animator = GetComponentInChildren<Animator>(true);
            if (_enemy == null || _animator == null ||
                !_animator.HasState(0, IdleState) || !_animator.HasState(0, WalkState))
            {
                Debug.LogWarning($"{name}: 순찰 애니메이션에 EnemyCore와 Idle/Walk 상태가 필요합니다.", this);
                enabled = false;
                return;
            }

            _hasIdle2State = _animator.HasState(0, Idle2State);
            if (!_hasIdle2State)
                Debug.LogWarning($"{name}: Animator에 Idle2 상태가 없어 두 번째 대기는 Idle로 표시됩니다.", this);
            _hasRunState = _animator.HasState(0, RunState);
            if (!_hasRunState)
                Debug.LogWarning($"{name}: Animator에 Run 상태가 없어 추적 이동은 Walk로 표시됩니다.", this);
        }

        private void Update()
        {
            EEnemyState state = _enemy.StateFlow.CurrentState;
            int nextState;
            if (state == EEnemyState.Patrol)
            {
                nextState = _enemy.StateFlow.PatrolPhase switch
                {
                    EEnemyPatrolPhase.Idle2 => _hasIdle2State ? Idle2State : IdleState,
                    EEnemyPatrolPhase.Moving => _enemy.Locomotion != null &&
                                                _enemy.Locomotion.CurrentSpeed > _walkSpeedThreshold
                        ? WalkState : IdleState,
                    _ => IdleState
                };
            }
            else if (state == EEnemyState.Chase)
            {
                nextState = _enemy.Locomotion != null &&
                            _enemy.Locomotion.CurrentSpeed > _walkSpeedThreshold
                    ? (_hasRunState ? RunState : WalkState) : IdleState;
            }
            else if (state == EEnemyState.Idle || _currentState == WalkState ||
                     _currentState == Idle2State || _currentState == RunState)
                nextState = IdleState;
            else
                return;

            if (nextState == _currentState) return;
            _animator.CrossFade(nextState, _transitionDuration, 0);
            _currentState = nextState;
        }
    }
}
