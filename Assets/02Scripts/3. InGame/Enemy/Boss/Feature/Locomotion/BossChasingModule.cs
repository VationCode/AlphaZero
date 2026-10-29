using UnityEngine;

namespace alpha.ingame.enemy.boss
{
    public enum EBossChaseResult
    {
        Running,
        Reached,
        TargetLost,
        NoProgress,
        MovementUnavailable
    }

    /// <summary>타깃을 향해 이동하고 시전 거리까지의 진행 여부를 측정한다.</summary>
    [DisallowMultipleComponent]
    public sealed class BossChasingModule : MonoBehaviour
    {
        private const float ArrivalTolerance = 0.05f;

        [Header("No Progress")]
        [SerializeField] private bool _useNoProgress = true;
        [Tooltip("이 시간 동안 충분히 가까워지지 않으면 추적을 중단한다. 0이면 적용하지 않는다.")]
        [SerializeField, Min(0f)] private float _noProgressDuration = 3f;
        [Tooltip("시전 거리까지 남은 거리가 이 값 이상 줄어야 진행한 것으로 본다.")]
        [SerializeField, Min(0.01f)] private float _progressDistance = 0.5f;

        private BossLocomotionModule _locomotion;
        private Transform _target;
        private float _stopDistance;
        private float _bestGap;
        private float _lastProgressAt;

        public float ProgressDistance => Mathf.Max(0.01f, _progressDistance);
        public float CurrentDistance => _target != null && _locomotion != null
            ? HorizontalDistance(_locomotion.Position, _target.position) : float.PositiveInfinity;

        public void Bind(BossLocomotionModule locomotion)
        {
            Stop();
            _locomotion = locomotion;
        }

        public bool Begin(Transform target, float stopDistance)
        {
            if (!isActiveAndEnabled || _locomotion == null || !_locomotion.CanMove ||
                target == null || !target.gameObject.activeInHierarchy) return false;

            _target = target;
            _stopDistance = Mathf.Max(0f, stopDistance);
            _bestGap = Mathf.Max(0f, CurrentDistance - _stopDistance);
            _lastProgressAt = Time.time;
            return true;
        }

        // ChasingState의 물리 프레임에서 호출한다.
        public EBossChaseResult Tick()
        {
            if (_target == null || !_target.gameObject.activeInHierarchy)
                return EBossChaseResult.TargetLost;
            if (!isActiveAndEnabled || _locomotion == null || !_locomotion.CanMove)
                return EBossChaseResult.MovementUnavailable;

            float distance = CurrentDistance;
            // 공격 후보를 정한 뒤에는 먼저 정면을 맞추고, 그 다음 이동한다.
            if (!_locomotion.Face(_target.position))
            {
                _locomotion.Stop();
                _lastProgressAt = Time.time;
                return EBossChaseResult.Running;
            }

            if (distance <= _stopDistance + ArrivalTolerance)
            {
                _locomotion.Stop();
                return EBossChaseResult.Reached;
            }

            float gap = distance - _stopDistance;
            if (gap <= _bestGap - ProgressDistance)
            {
                _bestGap = gap;
                _lastProgressAt = Time.time;
            }
            if (_useNoProgress && _noProgressDuration > 0f &&
                Time.time - _lastProgressAt >= _noProgressDuration)
            {
                _locomotion.Stop();
                return EBossChaseResult.NoProgress;
            }

            _locomotion.MoveTo(_target.position, EBossMovePace.Run, _stopDistance);
            return EBossChaseResult.Running;
        }

        public void Stop()
        {
            _locomotion?.Stop();
            _target = null;
            _bestGap = 0f;
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            Vector3 offset = b - a;
            offset.y = 0f;
            return offset.magnitude;
        }

        private void OnDisable() => Stop();

        private void OnValidate()
        {
            _noProgressDuration = Mathf.Max(0f, _noProgressDuration);
            _progressDistance = Mathf.Max(0.01f, _progressDistance);
        }
    }
}
