using UnityEngine;
using UnityEngine.AI;

namespace alpha.ingame.enemy
{
    public enum EEnemyChaseCancelReason
    {
        None,
        TargetLost,
        Distance,
        SightLost,
        ActivityArea,
        Duration,
        NoProgress,
        PathFailed,
        CombatArea
    }

    /// <summary>추적 이동과 대상 기록을 관리하고 취소 조건을 평가한다.</summary>
    public sealed class EnemyChaseModule : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0.01f)] private float _chaseSpeed = 6f;
        [SerializeField, Min(0.01f), InspectorName("Stop Distance")]
        private float _chaseStopDistance = 2f;
        [SerializeField] private bool _showChaseStopDistance = true;
        [SerializeField] private Color _chaseStopColor = new Color(0.7f, 0.3f, 1f, 0.9f);

        [Header("General Cancel")]
        [SerializeField, Min(0f)] private float _releaseDistance = 20f;
        [SerializeField, Min(0f)] private float _lostSightDuration = 2f;
        [SerializeField, Min(0f)] private float _pathFailureDuration = 0.5f;

        [Header("Optional Cancel (0 = Off)")]
        [SerializeField, Min(0f)] private float _activityRadius = 25f;
        [SerializeField, Min(0f)] private float _maxChaseDuration;
        [SerializeField, Min(0f)] private float _noProgressDuration;
        [SerializeField, Min(0.01f)] private float _progressDistance = 0.5f;
        [SerializeField] private BoxCollider _combatArea;

        private EnemyCore _enemy;
        private EnemyLocomotionModule _locomotion;
        private NavMeshAgent _agent;
        private Vector3 _destination;
        private bool _hasDestination;
        private bool _moving;
        private Vector3 _previousPosition;
        private bool _hasMovementSample;
        private Vector3 _activityOrigin;
        private float _startedAt;
        private float _lastSeenAt;
        private float _lastProgressAt;
        private float _bestDistance;
        private float _pathFailureStartedAt = -1f;

        public Transform Target { get; private set; }
        public Vector3 LastSeenPosition { get; private set; }
        public bool CanMove => _agent != null && _agent.enabled && _agent.isOnNavMesh;
        public float StopDistance => Mathf.Max(0.01f, _chaseStopDistance);
        public bool PathFailed => CanMove && _moving && _hasDestination &&
                                  !_agent.pathPending &&
                                  (_agent.pathStatus != NavMeshPathStatus.PathComplete ||
                                   (!_agent.hasPath &&
                                    HorizontalDistance(_agent.transform.position, _destination) >
                                    StopDistance));

        private void Awake()
        {
            _enemy = GetComponentInParent<EnemyCore>(true);
            _locomotion = GetComponent<EnemyLocomotionModule>();
            if (_enemy != null) _agent = _enemy.GetComponent<NavMeshAgent>();
            if (_locomotion == null || _agent == null)
                Debug.LogError($"{name}: 추적에 EnemyLocomotionModule과 NavMeshAgent가 필요합니다.", this);
        }

        public void Begin(Transform target, Vector3 activityOrigin, Vector3 enemyPosition)
        {
            _moving = false;
            _hasDestination = false;
            _hasMovementSample = false;
            Target = target;
            _activityOrigin = activityOrigin;
            _startedAt = Time.time;
            _lastSeenAt = Time.time;
            _lastProgressAt = Time.time;
            _pathFailureStartedAt = -1f;
            LastSeenPosition = target != null ? target.position : enemyPosition;
            _destination = LastSeenPosition;
            _bestDistance = target != null
                ? HorizontalDistance(enemyPosition, target.position) : 0f;
        }

        public EEnemyChaseCancelReason Evaluate(Vector3 enemyPosition, bool visible,
            bool atTarget, bool pathFailed)
        {
            if (Target == null || !Target.gameObject.activeInHierarchy)
                return EEnemyChaseCancelReason.TargetLost;

            float now = Time.time;
            float distance = HorizontalDistance(enemyPosition, Target.position);
            if (_releaseDistance > 0f && distance > _releaseDistance)
                return EEnemyChaseCancelReason.Distance;
            if (_activityRadius > 0f &&
                HorizontalDistance(enemyPosition, _activityOrigin) > _activityRadius)
                return EEnemyChaseCancelReason.ActivityArea;
            if (_combatArea != null && !IsInsideCombatArea(Target.position))
                return EEnemyChaseCancelReason.CombatArea;
            if (_maxChaseDuration > 0f && now - _startedAt >= _maxChaseDuration)
                return EEnemyChaseCancelReason.Duration;

            if (visible)
            {
                _lastSeenAt = now;
                LastSeenPosition = Target.position;
            }
            else if (now - _lastSeenAt >= _lostSightDuration)
                return EEnemyChaseCancelReason.SightLost;

            if (atTarget || distance <= _bestDistance - _progressDistance)
            {
                _bestDistance = distance;
                _lastProgressAt = now;
            }
            if (_noProgressDuration > 0f && !atTarget &&
                now - _lastProgressAt >= _noProgressDuration)
                return EEnemyChaseCancelReason.NoProgress;

            if (atTarget || !pathFailed)
                _pathFailureStartedAt = -1f;
            else if (_pathFailureStartedAt < 0f)
                _pathFailureStartedAt = now;
            if (_pathFailureStartedAt >= 0f &&
                now - _pathFailureStartedAt >= _pathFailureDuration)
                return EEnemyChaseCancelReason.PathFailed;

            return EEnemyChaseCancelReason.None;
        }

        public void End()
        {
            StopMovement();
            Target = null;
            _pathFailureStartedAt = -1f;
        }

        public bool HasReachedTarget(Vector3 targetPosition)
        {
            if (_enemy == null || !CanMove) return false;
            return HorizontalDistance(_enemy.transform.position, targetPosition) <= StopDistance;
        }

        public bool MoveTowards(Vector3 destination)
        {
            if (_enemy == null || _enemy.IsDead || !CanMove) return false;
            bool wasMoving = _moving;
            _agent.speed = Mathf.Max(0.01f, _chaseSpeed);
            _agent.stoppingDistance = 0f;
            _agent.isStopped = false;

            if (_moving && _hasDestination && (_agent.hasPath || _agent.pathPending) &&
                (destination - _destination).sqrMagnitude < 0.01f)
            {
                _destination = destination;
                return true;
            }
            if (!_agent.SetDestination(destination))
            {
                StopMovement();
                return false;
            }

            _destination = destination;
            _hasDestination = true;
            _moving = true;
            if (!wasMoving)
            {
                _previousPosition = _enemy.transform.position;
                _hasMovementSample = true;
            }
            return true;
        }

        public void StopAtBoundary(Vector3 targetPosition)
        {
            _destination = targetPosition;
            if (_moving) TryClampToBoundary(targetPosition);
            StopMovement();
        }

        private void StopMovement()
        {
            _moving = false;
            _hasDestination = false;
            _hasMovementSample = false;
            _locomotion?.Stop();
        }

        private void LateUpdate()
        {
            if (!_moving || !CanMove || !_hasMovementSample) return;
            if (_hasDestination && TryClampToBoundary(_destination))
            {
                StopMovement();
                return;
            }
            _previousPosition = _enemy.transform.position;
        }

        private bool TryClampToBoundary(Vector3 targetPosition)
        {
            if (!CanMove || !_hasMovementSample) return false;

            Vector3 current = _enemy.transform.position;
            Vector3 from = _previousPosition - targetPosition;
            Vector3 to = current - targetPosition;
            from.y = 0f;
            to.y = 0f;
            float radiusSquared = StopDistance * StopDistance;
            if (from.sqrMagnitude <= radiusSquared || to.sqrMagnitude >= radiusSquared)
                return false;

            // 한 프레임에 정지 원을 넘어가면 이동 경로와 원의 교점으로 되돌린다.
            Vector3 step = to - from;
            float a = Vector3.Dot(step, step);
            if (a < 0.000001f) return false;
            float b = 2f * Vector3.Dot(from, step);
            float discriminant = b * b - 4f * a * (from.sqrMagnitude - radiusSquared);
            if (discriminant < 0f) return false;

            float fraction = Mathf.Clamp01((-b - Mathf.Sqrt(discriminant)) / (2f * a));
            return _agent.Warp(Vector3.Lerp(_previousPosition, current, fraction));
        }

        private bool IsInsideCombatArea(Vector3 position)
        {
            Vector3 local = _combatArea.transform.InverseTransformPoint(position) - _combatArea.center;
            Vector3 halfSize = _combatArea.size * 0.5f;
            return Mathf.Abs(local.x) <= halfSize.x && Mathf.Abs(local.z) <= halfSize.z;
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            Vector3 offset = a - b;
            offset.y = 0f;
            return offset.magnitude;
        }

        private void OnValidate()
        {
            _chaseSpeed = Mathf.Max(0.01f, _chaseSpeed);
            _chaseStopDistance = Mathf.Max(0.01f, _chaseStopDistance);
            if (Application.isPlaying && _moving && _agent != null)
                _agent.speed = _chaseSpeed;
            _releaseDistance = Mathf.Max(0f, _releaseDistance);
            _lostSightDuration = Mathf.Max(0f, _lostSightDuration);
            _pathFailureDuration = Mathf.Max(0f, _pathFailureDuration);
            _activityRadius = Mathf.Max(0f, _activityRadius);
            _maxChaseDuration = Mathf.Max(0f, _maxChaseDuration);
            _noProgressDuration = Mathf.Max(0f, _noProgressDuration);
            _progressDistance = Mathf.Max(0.01f, _progressDistance);
        }

        private void OnDrawGizmos()
        {
            if (!_showChaseStopDistance) return;
            EnemyCore enemy = _enemy != null ? _enemy : GetComponentInParent<EnemyCore>(true);
            if (enemy == null) return;

            Collider body = enemy.GetComponent<Collider>();
            float height = body != null ? body.bounds.center.y - enemy.transform.position.y : 1f;
            Vector3 center = enemy.transform.position + Vector3.up * height;
            if (Application.isPlaying && enemy.StateFlow != null &&
                enemy.StateFlow.CurrentState == EEnemyState.Chase)
            {
                center = _destination + Vector3.up * height;
                Gizmos.color = _chaseStopColor;
                Vector3 enemyCenter = enemy.transform.position + Vector3.up * height;
                Gizmos.DrawLine(enemyCenter, center);
                Gizmos.DrawSphere(enemyCenter, 0.08f);
            }
            center.y += 0.05f;
            Gizmos.color = _chaseStopColor;

            const int segments = 48;
            Vector3 previous = center + Vector3.right * StopDistance;
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector3 next = center + new Vector3(Mathf.Cos(angle), 0f,
                    Mathf.Sin(angle)) * StopDistance;
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
    }
}
