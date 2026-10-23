using UnityEngine;
using alpha.ingame;

namespace alpha.ingame.player
{
    /// <summary>빔포드 대형이 플레이어의 위치와 방향을 부드럽게 따라가게 한다.</summary>
    [DefaultExecutionOrder(150)]
    public sealed class BeamPodFollow : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float _followSmoothTime = 0.18f;
        [SerializeField, Min(0f)] private float _turnSpeed = 180f;

        private readonly SmoothPositionFollower _positionFollower = new();
        private SummonLifetime[] _pods;
        private Transform _player;
        private Vector3 _formationOffset;
        private Vector3 _currentPosition;
        private Quaternion _currentRotation;
        private bool _hasFormationOffset;
        private bool _isFollowing;

        private void Awake()
        {
            _pods = GetComponentsInChildren<SummonLifetime>(true);
            _player = GetComponentInParent<PlayerCore>(true)?.transform;
        }

        public bool Begin()
        {
            if (_player == null) _player = GetComponentInParent<PlayerCore>(true)?.transform;
            if (_player == null)
            {
                Debug.LogError($"{nameof(BeamPodFollow)}: 플레이어가 없습니다.", this);
                return false;
            }

            if (!_hasFormationOffset)
            {
                _formationOffset = _player.InverseTransformPoint(transform.position);
                _hasFormationOffset = true;
            }

            // 소환 직전 대형을 플레이어 방향으로 배치하고 이전 추적 속도를 지운다.
            _positionFollower.Reset();
            FollowPlayer(true);
            _isFollowing = true;
            return true;
        }

        private void LateUpdate()
        {
            if (!_isFollowing) return;

            foreach (SummonLifetime pod in _pods)
            {
                if (pod == null || !pod.gameObject.activeInHierarchy) continue;
                FollowPlayer(false);
                return;
            }

            Stop();
        }

        private void OnDisable() => Stop();

        private void Stop()
        {
            _isFollowing = false;
            _positionFollower.Reset();
        }

        private void FollowPlayer(bool snap)
        {
            Vector3 forward = Vector3.ProjectOnPlane(_player.forward, Vector3.up);
            Quaternion targetRotation = forward.sqrMagnitude >= 0.0001f
                ? Quaternion.LookRotation(forward.normalized, Vector3.up)
                : snap ? transform.rotation : _currentRotation;

            // 부모인 플레이어의 회전이 매 프레임 적용되므로 독립된 월드 자세를 기준으로 보간한다.
            Quaternion nextRotation = snap
                ? targetRotation
                : Quaternion.RotateTowards(_currentRotation, targetRotation,
                    _turnSpeed * Time.deltaTime);
            Vector3 targetPosition = _player.position + nextRotation * _formationOffset;
            Vector3 nextPosition = snap
                ? targetPosition
                : _positionFollower.Step(_currentPosition, targetPosition, _followSmoothTime);
            _currentPosition = nextPosition;
            _currentRotation = nextRotation;
            transform.SetPositionAndRotation(nextPosition, nextRotation);
        }
    }
}
