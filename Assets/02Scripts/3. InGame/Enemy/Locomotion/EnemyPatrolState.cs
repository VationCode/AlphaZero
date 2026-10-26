using UnityEngine;

namespace alpha.ingame.enemy
{
    public enum EEnemyPatrolPhase
    {
        Selecting,
        Moving,
        Idle,
        Idle2
    }

    /// <summary>전방 지점을 탐색하고 더 갈 곳이 없으면 시작점으로 돌아온다.</summary>
    public sealed class EnemyPatrolState : EnemyState
    {
        private const float RetryDelay = 1f;

        private float _phaseEndTime;
        private bool _hasVisitedPoint;
        private bool _returningToStart;
        private bool _warnedNoDestination;

        public EEnemyPatrolPhase Phase { get; private set; } = EEnemyPatrolPhase.Selecting;

        public EnemyPatrolState(EnemyCore enemy, EnemyStateFlow flow) : base(enemy, flow) { }

        public override void Enter()
        {
            Enemy.Locomotion?.Stop();
            _hasVisitedPoint = false;
            _returningToStart = false;
            _warnedNoDestination = false;
            Phase = EEnemyPatrolPhase.Selecting;
            _phaseEndTime = Time.time;
        }

        public void RequestReturnToStart()
        {
            _returningToStart = true;
            _hasVisitedPoint = true;
            _warnedNoDestination = false;
            Phase = EEnemyPatrolPhase.Selecting;
            _phaseEndTime = Time.time;
        }

        public override void Tick()
        {
            if (Enemy.Combat?.Target != null)
            {
                Flow.ChangeState(EEnemyState.Chase);
                return;
            }

            EnemyPatrolArea patrolArea = Flow.PatrolArea;
            if (patrolArea == null || !patrolArea.IsReady || Enemy.Locomotion == null ||
                !Enemy.Locomotion.CanMove)
            {
                Flow.ChangeState(EEnemyState.Idle);
                return;
            }

            if (Phase == EEnemyPatrolPhase.Moving)
            {
                if (Enemy.Locomotion.HasReachedPatrolDestination())
                {
                    Enemy.Locomotion.Stop();
                    if (_returningToStart)
                    {
                        _hasVisitedPoint = false;
                        _returningToStart = false;
                    }
                    else _hasVisitedPoint = true;

                    BeginArrivalWait(patrolArea);
                }
                else if (Enemy.Locomotion.PathFailed)
                {
                    Enemy.Locomotion.Stop();
                    Phase = EEnemyPatrolPhase.Selecting;
                    _phaseEndTime = Time.time + RetryDelay;
                }
                return;
            }

            if (Phase == EEnemyPatrolPhase.Idle)
            {
                if (Time.time < _phaseEndTime) return;
                BeginIdle2(patrolArea);
            }

            if (Phase == EEnemyPatrolPhase.Idle2)
            {
                if (Time.time < _phaseEndTime) return;
                Phase = EEnemyPatrolPhase.Selecting;
            }

            if (Time.time < _phaseEndTime) return;

            Vector3 destination;
            bool found;
            if (_returningToStart)
            {
                found = patrolArea.TryGetStartDestination(out destination);
            }
            else
            {
                found = patrolArea.TryGetRandomDestination(out destination);
                if (!found && _hasVisitedPoint)
                {
                    // 전방에서 다음 지점을 찾지 못하면 지점 수와 관계없이 복귀한다.
                    _returningToStart = true;
                    found = patrolArea.TryGetStartDestination(out destination);
                }
            }

            if (found && Enemy.Locomotion.MovePatrolTowards(destination))
            {
                Phase = EEnemyPatrolPhase.Moving;
                _warnedNoDestination = false;
            }
            else
            {
                if (!_warnedNoDestination)
                {
                    Debug.LogWarning($"{Enemy.name}: 도달 가능한 순찰 목적지를 찾지 못했습니다.",
                        Enemy);
                    _warnedNoDestination = true;
                }
                _phaseEndTime = Time.time + RetryDelay;
            }
        }

        private void BeginArrivalWait(EnemyPatrolArea patrolArea)
        {
            if (patrolArea.IdleDuration <= 0f)
            {
                BeginIdle2(patrolArea);
                return;
            }

            Phase = EEnemyPatrolPhase.Idle;
            _phaseEndTime = Time.time + patrolArea.IdleDuration;
        }

        private void BeginIdle2(EnemyPatrolArea patrolArea)
        {
            Phase = patrolArea.Idle2Duration > 0f
                ? EEnemyPatrolPhase.Idle2 : EEnemyPatrolPhase.Selecting;
            _phaseEndTime = Time.time + patrolArea.Idle2Duration;
        }

        public override void Exit() => Enemy.Locomotion?.Stop();
    }
}
