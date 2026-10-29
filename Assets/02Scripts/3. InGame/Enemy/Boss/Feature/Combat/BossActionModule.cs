using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace alpha.ingame.enemy.boss
{
    /// <summary>보스 행동의 구체적인 처리를 모으는 모듈이다.</summary>
    [DisallowMultipleComponent]
    public sealed class BossActionModule : MonoBehaviour
    {
        [Header("Target Search")]
        [SerializeField] private LayerMask _targetLayers = 1 << 7;

        [Header("Attack Cast Conditions")]
        [SerializeField] private BossAttackCastConditions[] _attackConditions =
            Array.Empty<BossAttackCastConditions>();

        [Header("Attack5 Butt")]
        [Tooltip("ButtStart 위치에서 보스가 정면으로 전진할 거리입니다. CastDistance와 합산하지 않습니다.")]
        [SerializeField, Min(0f)] private float _attack5ButtDistance = 2.62f;
        [SerializeField] private bool _showAttack5ButtDistance = true;

        private readonly Collider[] _targetCandidates = new Collider[32];
        private Transform _boss;

        public Transform Owner => _boss;
        public Transform CurrentTarget { get; private set; }
        public bool HasCombatTarget => CurrentTarget != null &&
            CurrentTarget.gameObject.activeInHierarchy &&
            (_targetLayers.value & (1 << CurrentTarget.gameObject.layer)) != 0;
        public IReadOnlyList<BossAttackCastConditions> AttackConditions => _attackConditions;
        public float Attack5ButtDistance => _attack5ButtDistance;

        public void Bind(Transform boss) => _boss = boss;

        // Player 레이어의 충돌체에서 신체 중심으로 사용할 CharacterController 루트를 찾는다.
        public bool TryAcquireCombatTarget(float maxDistance)
        {
            if (_boss != null && maxDistance > 0f && HasCombatTarget)
            {
                Vector3 offset = CurrentTarget.position - _boss.position;
                offset.y = 0f;
                if (offset.sqrMagnitude <= maxDistance * maxDistance) return true;
            }
            CurrentTarget = null;
            if (!isActiveAndEnabled || _boss == null || maxDistance <= 0f ||
                _targetLayers.value == 0) return false;

            int count = Physics.OverlapSphereNonAlloc(_boss.position, maxDistance,
                _targetCandidates, _targetLayers, QueryTriggerInteraction.Ignore);
            float nearestSqrDistance = maxDistance * maxDistance;
            Transform nearest = null;
            for (int i = 0; i < count; i++)
            {
                Collider candidate = _targetCandidates[i];
                if (candidate == null || !candidate.enabled) continue;

                CharacterController controller = candidate.GetComponentInParent<CharacterController>();
                if (controller == null || !controller.enabled ||
                    !controller.gameObject.activeInHierarchy ||
                    (_targetLayers.value & (1 << controller.gameObject.layer)) == 0) continue;

                Vector3 offset = controller.transform.position - _boss.position;
                offset.y = 0f;
                float sqrDistance = offset.sqrMagnitude;
                if (sqrDistance > nearestSqrDistance) continue;
                nearestSqrDistance = sqrDistance;
                nearest = controller.transform;
            }

            CurrentTarget = nearest;
            return nearest != null;
        }

        public bool TryGetAttackConditions(string attackId,
            out BossAttackCastConditions conditions)
        {
            conditions = null;
            if (_attackConditions == null) return false;
            foreach (BossAttackCastConditions candidate in _attackConditions)
            {
                if (candidate == null || candidate.AttackId != attackId) continue;
                conditions = candidate;
                return true;
            }
            return false;
        }

        private void OnValidate()
        {
            _attack5ButtDistance = Mathf.Max(0f, _attack5ButtDistance);
            if (_attackConditions == null) return;

            foreach (BossAttackCastConditions conditions in _attackConditions)
                conditions?.Validate();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Camera.current == null || Camera.current.cameraType != CameraType.SceneView) return;

            Transform owner = _boss != null ? _boss : GetComponentInParent<BossCore>(true)?.transform;
            if (owner == null) return;
            Vector3 center = owner.position + Vector3.up * 0.1f;
            Vector3 forward = Vector3.ProjectOnPlane(owner.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
            forward.Normalize();

            if (_attackConditions != null)
            {
                for (int i = 0; i < _attackConditions.Length; i++)
                {
                    BossAttackCastConditions conditions = _attackConditions[i];
                    if (conditions == null ||
                        (!conditions.ShowSelectionDistance && !conditions.ShowCastDistance)) continue;
                    DrawCastConditionsGizmo(center, forward, conditions, i);
                }
            }

            if (_showAttack5ButtDistance && _attack5ButtDistance > 0f)
                DrawAttack5ButtGizmo(center, forward, _attack5ButtDistance);
        }

        private static void DrawAttack5ButtGizmo(Vector3 start, Vector3 forward,
            float distance)
        {
            // 편집 중에는 현재 보스 위치·정면을 ButtStart 순간의 미리보기로 사용한다.
            Vector3 end = start + forward * distance;
            Color previous = Handles.color;
            Handles.color = new Color(1f, 0.65f, 0f, 1f);
            Handles.DrawLine(start, end);
            Handles.DrawWireDisc(end, Vector3.up, 0.25f);
            Handles.Label(end + Vector3.up * 0.35f, $"Attack5 butt +{distance:0.##}m");
            Handles.color = previous;
        }

        private static void DrawCastConditionsGizmo(Vector3 center, Vector3 forward,
            BossAttackCastConditions conditions, int index)
        {
            float min = conditions.MinSelectionDistance;
            float max = conditions.MaxSelectionDistance;
            float cast = conditions.CastDistance;
            Color previous = Handles.color;

            if (conditions.ShowSelectionDistance && max > 0f)
            {
                Handles.color = conditions.SelectionDistanceColor;
                Handles.DrawWireDisc(center, Vector3.up, max);
                if (min > 0f)
                    Handles.DrawWireDisc(center, Vector3.up, min);
                Handles.Label(center + forward * max +
                    Vector3.up * (0.5f + index * 0.25f),
                    $"{conditions.AttackId} select {min:0.#}-{max:0.#}m");
            }

            if (conditions.ShowCastDistance && cast > 0f)
            {
                Handles.color = conditions.CastDistanceColor;
                Handles.DrawWireDisc(center + Vector3.up * 0.08f,
                    Vector3.up, cast);
                Handles.Label(center + forward * cast +
                    Vector3.up * (0.85f + index * 0.25f),
                    $"{conditions.AttackId} cast <= {cast:0.#}m");
            }

            Handles.color = previous;
        }

#endif
    }
}
