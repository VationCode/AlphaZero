using System.Collections.Generic;
using UnityEngine;

namespace alpha.ingame.enemy.boss
{
    /// <summary>공격 시작 조건과 애니메이션 종료 후 쿨다운을 관리한다.</summary>
    public sealed class BossAttackingState
    {
        public const float CastTolerance = 0.05f;
        private readonly Dictionary<string, float> _readyAt = new();
        private BossAttackCastConditions _currentAttack;

        public bool IsReady(BossAttackCastConditions attack)
        {
            if (attack == null || !attack.IsEnabled ||
                string.IsNullOrEmpty(attack.AttackId)) return false;

            return !_readyAt.TryGetValue(attack.AttackId, out float readyAt) ||
                Time.time >= readyAt;
        }

        public bool CanStart(BossAttackCastConditions attack, Transform boss, Transform target)
        {
            if (!IsReady(attack) || boss == null || target == null ||
                !target.gameObject.activeInHierarchy)
                return false;

            Vector3 direction = target.position - boss.position;
            direction.y = 0f;
            float distance = direction.magnitude;
            if (distance < attack.MinSelectionDistance ||
                distance > attack.MaxSelectionDistance ||
                distance > attack.CastDistance + CastTolerance) return false;

            return true;
        }

        public bool Enter(BossAttackCastConditions attack, Transform boss, Transform target)
        {
            if (!CanStart(attack, boss, target)) return false;
            _currentAttack = attack;
            return true;
        }

        public bool Complete()
        {
            if (_currentAttack == null) return false;
            _readyAt[_currentAttack.AttackId] = Time.time + _currentAttack.Cooldown;
            _currentAttack = null;
            return true;
        }

        public void Exit() => _currentAttack = null;
    }
}
