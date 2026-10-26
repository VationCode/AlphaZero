using UnityEngine;
using UnityEngine.AI;

namespace alpha.ingame.enemy
{
    /// <summary>NavMeshAgent로 순찰 이동과 공통 정지·회전을 실행한다.</summary>
    public sealed class EnemyLocomotionModule : MonoBehaviour
    {
        private const float PatrolArrivalTolerance = 0.25f;

        [Header("Patrol Movement")]
        [SerializeField, Min(0.01f), InspectorName("Patrol Speed")]
        private float _moveSpeed = 3.5f;

        private EnemyCore _enemy;
        private NavMeshAgent _agent;
        private Vector3 _destination;
        private float _destinationRequestedAt;
        private bool _hasDestination;
        public bool CanMove => _agent != null && _agent.enabled && _agent.isOnNavMesh;
        public float CurrentSpeed => CanMove && !_agent.isStopped ? _agent.velocity.magnitude : 0f;
        public bool PathFailed => CanMove && _hasDestination && !_agent.pathPending &&
                                  Time.time - _destinationRequestedAt > 0.5f &&
                                  (_agent.pathStatus != NavMeshPathStatus.PathComplete ||
                                   (!_agent.hasPath && HorizontalDistanceToDestination >
                                    PatrolArrivalTolerance));

        private float HorizontalDistanceToDestination
        {
            get
            {
                Vector3 offset = _destination - _agent.transform.position;
                offset.y = 0f;
                return offset.magnitude;
            }
        }

        private void Awake()
        {
            _enemy = GetComponentInParent<EnemyCore>(true);
            if (_enemy != null) _agent = _enemy.GetComponent<NavMeshAgent>();
            if (_agent == null)
                Debug.LogError($"{name}: 적 루트에 NavMeshAgent가 필요합니다.", this);
            else _agent.speed = Mathf.Max(0.01f, _moveSpeed);
        }

        private void OnValidate()
        {
            _moveSpeed = Mathf.Max(0.01f, _moveSpeed);
        }

        public bool MovePatrolTowards(Vector3 destination)
        {
            if (_enemy == null || _enemy.IsDead || !CanMove) return false;
            _agent.speed = Mathf.Max(0.01f, _moveSpeed);
            _agent.stoppingDistance = 0f;
            _agent.isStopped = false;
            if (!_agent.SetDestination(destination))
            {
                Stop();
                return false;
            }
            _destination = destination;
            _destinationRequestedAt = Time.time;
            _hasDestination = true;
            return true;
        }

        public void Face(Vector3 position)
        {
            if (_enemy == null || _enemy.IsDead) return;
            Transform root = _enemy.transform;
            Vector3 direction = position - root.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float turnSpeed = _agent != null ? _agent.angularSpeed : 540f;
            root.rotation = Quaternion.RotateTowards(root.rotation, targetRotation,
                turnSpeed * Time.deltaTime);
        }

        public bool HasReachedPatrolDestination()
        {
            if (!CanMove || !_hasDestination || _agent.pathPending ||
                HorizontalDistanceToDestination > PatrolArrivalTolerance)
                return false;

            // 순찰은 목적지까지 이동하고 NavMesh의 작은 위치 오차만 허용한다.
            return !_agent.hasPath ||
                   (!float.IsInfinity(_agent.remainingDistance) &&
                    _agent.remainingDistance <= PatrolArrivalTolerance);
        }

        public void Stop()
        {
            _hasDestination = false;
            if (!CanMove) return;
            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.velocity = Vector3.zero;
            _agent.speed = Mathf.Max(0.01f, _moveSpeed);
        }
    }
}
