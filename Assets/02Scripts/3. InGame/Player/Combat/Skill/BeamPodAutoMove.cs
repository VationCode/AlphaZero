using UnityEngine;

namespace alpha.ingame.player
{
    /// <summary>빔포드를 무작위 지점 사이로 이동시키고 타겟팅 상태에 따라 범위를 바꾼다.</summary>
    [DefaultExecutionOrder(155)]
    [RequireComponent(typeof(SummonLifetime))]
    public sealed class BeamPodAutoMove : MonoBehaviour
    {
        [Header("Auto Move")]
        [SerializeField, Min(2)] private int _pointCount = 3;
        [SerializeField, Min(0f)] private float _radius = 0.3f; // 타겟이 없을 때의 최대 거리
        [SerializeField, Min(1f)] private float _targetingMultiplier = 1.25f;
        [SerializeField, Min(0.01f)] private float _arrivalTime = 1.2f;
        [SerializeField, Range(0f, 1f)] private float _movementSmooth = 0.8f;

        private SummonLifetime _lifetime;
        private Vector3[] _pointOffsets;
        private Vector3 _originLocalPosition;
        private Vector3 _segmentStart;
        private Vector3 _segmentEnd;
        private float _segmentElapsed;
        private int _nextPoint;
        private bool _isMoving;
        private bool _hasTarget;

        private void Awake() => _lifetime = GetComponent<SummonLifetime>();

        public void Begin(Vector3 originLocalPosition)
        {
            _originLocalPosition = originLocalPosition;
            _hasTarget = false;
            _pointOffsets = new Vector3[Mathf.Max(2, _pointCount)];
            for (int i = 0; i < _pointOffsets.Length; i++)
                _pointOffsets[i] = Random.insideUnitSphere * Mathf.Max(0f, _radius);

            transform.localPosition = originLocalPosition;
            _nextPoint = 0;
            _isMoving = true;
            MoveToNextPoint();
        }

        public void SetTargeting(bool hasTarget)
        {
            if (!_isMoving || _hasTarget == hasTarget) return;
            _hasTarget = hasTarget;
            // 현재 위치에서 새 이동 범위의 목적지로 이어서 이동한다.
            MoveToNextPoint();
        }

        private void LateUpdate()
        {
            if (!_isMoving || _lifetime.IsExpired) return;

            _segmentElapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_segmentElapsed / Mathf.Max(0.01f, _arrivalTime));
            float eased = Mathf.SmoothStep(0f, 1f, progress);
            float blend = Mathf.Lerp(progress, eased, _movementSmooth);
            transform.localPosition = Vector3.LerpUnclamped(_segmentStart, _segmentEnd, blend);

            if (progress >= 1f) MoveToNextPoint();
        }

        private void MoveToNextPoint()
        {
            _segmentStart = transform.localPosition;
            float multiplier = _hasTarget ? Mathf.Max(1f, _targetingMultiplier) : 1f;
            _segmentEnd = _originLocalPosition + _pointOffsets[_nextPoint] * multiplier;
            _nextPoint = (_nextPoint + 1) % _pointOffsets.Length;
            _segmentElapsed = 0f;
        }

        private void OnDisable()
        {
            _isMoving = false;
            _hasTarget = false;
            transform.localPosition = _originLocalPosition;
        }
    }
}
