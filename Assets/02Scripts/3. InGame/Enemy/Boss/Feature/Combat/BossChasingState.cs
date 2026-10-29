using UnityEngine;

namespace alpha.ingame.enemy.boss
{
    /// <summary>선택된 공격의 추적을 유지하고 종료 시점을 판단한다.</summary>
    public sealed class BossChasingState
    {
        private BossActionFlow _flow;
        private BossChasingModule _chase;
        private Transform _target;
        private BossAttackCastConditions _attack;

        public void Bind(BossActionFlow flow, BossChasingModule chase)
        {
            Exit();
            _flow = flow;
            _chase = chase;
        }

        public bool Enter(Transform target, BossAttackCastConditions attack)
        {
            if (_chase == null || attack == null || !attack.IsEnabled ||
                !_chase.Begin(target, attack.CastDistance)) return false;

            _target = target;
            _attack = attack;
            return true;
        }

        public void FixedTick()
        {
            if (_flow == null || _chase == null || _attack == null) return;
            if (!_attack.IsEnabled)
            {
                _flow.TryChangeState(EBossActionState.Idle);
                return;
            }

            // 공격 선택 범위를 벗어나면 현재 공격의 추적만 끝낸다.
            float distance = _chase.CurrentDistance;
            if (_target != null &&
                (distance < _attack.MinSelectionDistance ||
                 distance > _attack.MaxSelectionDistance))
            {
                _flow.TryChangeState(EBossActionState.Idle);
                return;
            }

            EBossChaseResult result = _chase.Tick();
            if (result == EBossChaseResult.Running) return;

            if (result == EBossChaseResult.Reached &&
                _flow.TryBeginAttack(_target, _attack)) return;

            if (result == EBossChaseResult.NoProgress ||
                result == EBossChaseResult.MovementUnavailable)
                _flow.RecordChaseFailure(_attack, _target, distance,
                    _chase.ProgressDistance);

            // 조건이 달라졌거나 추적에 실패했으면 다시 공격을 선택한다.
            _flow.TryChangeState(EBossActionState.Idle);
        }

        public void Exit()
        {
            _chase?.Stop();
            _target = null;
            _attack = null;
        }
    }
}
