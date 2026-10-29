using UnityEngine;

namespace alpha.ingame.enemy.boss
{
    /// <summary>등록된 공격의 조건과 우선순위에 따라 공격 또는 추적을 요청한다.</summary>
    public sealed class BossIdleState
    {
        private BossActionFlow _flow;
        private BossActionModule _actions;

        public void Bind(BossActionFlow flow, BossActionModule actions)
        {
            _flow = flow;
            _actions = actions;
        }

        public void Tick()
        {
            if (_flow == null || _actions == null || _actions.Owner == null ||
                _actions.AttackConditions == null) return;

            // Inspector의 배열 순서가 공격 선택 우선순위다.
            float maxDistance = 0f;
            foreach (BossAttackCastConditions attack in _actions.AttackConditions)
                if (attack != null && attack.IsEnabled)
                    maxDistance = Mathf.Max(maxDistance, attack.MaxSelectionDistance);
            if (!_actions.TryAcquireCombatTarget(maxDistance)) return;

            Transform target = _actions.CurrentTarget;
            Vector3 offset = target.position - _actions.Owner.position;
            offset.y = 0f;
            float distance = offset.magnitude;

            foreach (BossAttackCastConditions attack in _actions.AttackConditions)
            {
                if (attack == null || !attack.IsEnabled || !_flow.IsAttackReady(attack) ||
                    distance < attack.MinSelectionDistance ||
                    distance > attack.MaxSelectionDistance) continue;

                if (distance <= attack.CastDistance + BossAttackingState.CastTolerance)
                {
                    if (_flow.TryBeginAttack(target, attack) ||
                        _flow.TryBeginChasing(target, attack)) return;
                    continue;
                }
                if (_flow.IsChaseBlocked(attack, target, distance)) continue;
                if (_flow.TryBeginChasing(target, attack)) return;
            }
        }
    }
}
