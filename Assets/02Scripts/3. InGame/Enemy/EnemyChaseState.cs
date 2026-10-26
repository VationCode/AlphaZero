using UnityEngine;

namespace alpha.ingame.enemy
{
    /// <summary>추적 모듈의 취소 판정에 따라 이동하거나 시작점으로 복귀한다.</summary>
    public sealed class EnemyChaseState : EnemyState
    {
        private bool _moveRequestFailed;

        public EnemyChaseState(EnemyCore enemy, EnemyStateFlow flow) : base(enemy, flow) { }

        public override void Enter()
        {
            _moveRequestFailed = false;
            Enemy.Chase?.Begin(Enemy.Combat?.Target, Enemy.ActivityOrigin,
                Enemy.transform.position);
        }

        public override void Tick()
        {
            EnemyChaseModule chase = Enemy.Chase;
            if (chase == null)
            {
                if (Enemy.Combat != null)
                    Enemy.Combat.SuppressTargetUntilOutOfRange(Enemy.Combat.Target);
                Flow.ReturnToPatrolOrigin();
                return;
            }

            Transform target = chase.Target;
            bool visible = target != null && Enemy.Combat != null && Enemy.Combat.CanSee(target);
            Vector3 destination = visible && target != null
                ? target.position : chase.LastSeenPosition;
            bool atTarget = chase.HasReachedTarget(destination);
            bool pathFailed = _moveRequestFailed || !chase.CanMove || chase.PathFailed;
            EEnemyChaseCancelReason reason = chase.Evaluate(Enemy.transform.position,
                visible, atTarget, pathFailed);
            if (reason != EEnemyChaseCancelReason.None)
            {
                // 시간·영역 등으로 취소한 대상은 감지 거리 밖으로 나갈 때까지 재추적하지 않는다.
                if (reason != EEnemyChaseCancelReason.SightLost &&
                    reason != EEnemyChaseCancelReason.TargetLost)
                    Enemy.Combat?.SuppressTargetUntilOutOfRange(target);
                Flow.ReturnToPatrolOrigin();
                return;
            }

            destination = visible ? target.position : chase.LastSeenPosition;
            if (atTarget)
            {
                chase.StopAtBoundary(destination);
                Enemy.Locomotion?.Face(destination);
                _moveRequestFailed = false;
                return;
            }

            if (!chase.CanMove)
            {
                _moveRequestFailed = true;
                return;
            }

            _moveRequestFailed = !chase.MoveTowards(destination);
        }

        public override void Exit()
        {
            if (Enemy.Chase != null) Enemy.Chase.End();
            else Enemy.Locomotion?.Stop();
        }
    }
}
