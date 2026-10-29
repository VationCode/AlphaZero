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

        [Header("Attack7 Summon")]
        [Tooltip("플레이어의 고정된 위치를 중심으로 소환체 전체가 들어갈 범위의 반경입니다.")]
        [SerializeField, Min(0f)] private float _attack7SpawnRadius = 8f;
        [Tooltip("Attack7의 EffectSpawn 이벤트 한 번에 생성할 소환체 수입니다.")]
        [SerializeField, Min(1)] private int _attack7SpawnCount = 6;
        [SerializeField] private bool _showAttack7SpawnRadius = true;
        [Tooltip("작업 씬에서 소환 범위의 중심으로 표시할 플레이어입니다.")]
        [SerializeField] private Transform _attack7PreviewTarget;

        private readonly Collider[] _targetCandidates = new Collider[32];
        private Transform _boss;

        public Transform Owner => _boss;
        public Transform CurrentTarget { get; private set; }
        public bool HasCombatTarget => CurrentTarget != null &&
            CurrentTarget.gameObject.activeInHierarchy &&
            (_targetLayers.value & (1 << CurrentTarget.gameObject.layer)) != 0;
        public IReadOnlyList<BossAttackCastConditions> AttackConditions => _attackConditions;
        public float Attack5ButtDistance => _attack5ButtDistance;

        // 타깃을 중심으로 서로 닿지 않는 무작위 소환 위치를 고른다.
        public void GetAttack7SpawnPositions(Vector3 center, float minSpacing,
            CharacterController player,
            List<Vector3> positions)
        {
            positions.Clear();
            float radius = Mathf.Max(0f, _attack7SpawnRadius - minSpacing * 0.5f);
            if (radius <= 0f)
            {
                Debug.LogWarning($"{name}: Attack7 소환 반경이 프리팹의 충돌 범위보다 작습니다.", this);
                return;
            }

            float minSpacingSqr = minSpacing * minSpacing;
            Vector3 playerCenter = player.transform.TransformPoint(player.center);
            Vector3 playerScale = player.transform.lossyScale;
            float horizontalScale = Mathf.Max(Mathf.Abs(playerScale.x), Mathf.Abs(playerScale.z));
            float playerClearance = minSpacing * 0.5f +
                (player.radius + player.skinWidth) * horizontalScale + 0.1f;
            float playerClearanceSqr = playerClearance * playerClearance;
            int maxAttempts = Mathf.Max(64, _attack7SpawnCount * 64);
            for (int attempt = 0; attempt < maxAttempts &&
                 positions.Count < _attack7SpawnCount; attempt++)
            {
                float angle = UnityEngine.Random.value * Mathf.PI * 2f;
                float distance = radius * Mathf.Sqrt(UnityEngine.Random.value);
                Vector3 candidate = center + new Vector3(
                    Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
                Vector3 toPlayer = candidate - playerCenter;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude < playerClearanceSqr) continue;

                bool overlaps = false;
                foreach (Vector3 placed in positions)
                {
                    Vector3 offset = candidate - placed;
                    offset.y = 0f;
                    if (offset.sqrMagnitude >= minSpacingSqr) continue;
                    overlaps = true;
                    break;
                }
                if (!overlaps) positions.Add(candidate);
            }

            if (positions.Count < _attack7SpawnCount)
                Debug.LogWarning($"{name}: Attack7 소환 범위에 {_attack7SpawnCount}마리를 " +
                    $"겹치지 않게 배치할 수 없어 {positions.Count}마리만 생성합니다.", this);
        }

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
            _attack7SpawnRadius = Mathf.Max(0f, _attack7SpawnRadius);
            _attack7SpawnCount = Mathf.Max(1, _attack7SpawnCount);
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

            if (_showAttack7SpawnRadius && _attack7SpawnRadius > 0f)
                DrawAttack7SpawnGizmo();
        }

        private void DrawAttack7SpawnGizmo()
        {
            Vector3? targetPosition = null;
            if (Application.isPlaying)
            {
                BossActionFlow flow = GetComponentInParent<BossCore>(true)?.Action;
                if (flow != null && flow.CurrentAttackId == "Attack7" &&
                    flow.TryGetTargetingPosition(out Vector3 lockedPosition))
                    targetPosition = lockedPosition;
            }

            if (!targetPosition.HasValue)
            {
                Transform target = Application.isPlaying && CurrentTarget != null
                    ? CurrentTarget : _attack7PreviewTarget;
                if (target == null) return;
                targetPosition = target.position;
            }

            Vector3 center = targetPosition.Value + Vector3.up * 0.1f;
            Color previous = Handles.color;
            Handles.color = new Color(1f, 0.35f, 0.8f, 1f);
            Handles.DrawWireDisc(center, Vector3.up, _attack7SpawnRadius);
            Handles.Label(center + Vector3.right * _attack7SpawnRadius +
                Vector3.up * 0.35f, $"Attack7 summon {_attack7SpawnRadius:0.#}m");
            Handles.color = previous;
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
