using System;
using UnityEngine;

namespace alpha.ingame.enemy.boss
{
    /// <summary>소환 위치를 중심으로 MiniCrab을 로컬 좌우 방향으로 왕복 이동시킨다.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class MiniCrabMotionModule : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _halfRange = 1.5f;
        [SerializeField, Min(0f)] private float _speed = 2f;

        private Rigidbody _body;
        private Vector3 _origin;
        private Vector3 _right;
        private float _travel;
        private float _previousOffset;
        private bool _started;
        private bool _spawnInitialized;
        private bool _stopped;

        public int Direction { get; private set; }
        public event Action<int> DirectionChanged;

        private void Awake() => _body = GetComponent<Rigidbody>();

        private void OnEnable()
        {
            _spawnInitialized = false;
            _travel = 0f;
            _previousOffset = 0f;
            _stopped = false;
            SetDirection(_halfRange > 0f && _speed > 0f ? 1 : 0);
            if (_started) InitializeSpawn();
        }

        private void Start()
        {
            _started = true;
            InitializeSpawn(); // Instantiate의 위치·회전 적용이 끝난 뒤 개체별 방향을 정한다.
        }

        private void InitializeSpawn()
        {
            Quaternion rotation = Quaternion.AngleAxis(
                UnityEngine.Random.Range(0f, 360f), Vector3.up) * transform.rotation;
            _body.rotation = rotation;
            transform.rotation = rotation;
            _origin = _body.position;
            _right = rotation * Vector3.right;
            _spawnInitialized = true;
        }

        private void FixedUpdate()
        {
            if (!_spawnInitialized || _stopped || _halfRange <= 0f || _speed <= 0f) return;

            _travel = Mathf.Repeat(_travel + _speed * Time.fixedDeltaTime, 4f * _halfRange);
            float offset = Mathf.PingPong(_travel + _halfRange, 2f * _halfRange) - _halfRange;
            if (offset > _previousOffset + 0.0001f) SetDirection(1);
            else if (offset < _previousOffset - 0.0001f) SetDirection(-1);

            _body.MovePosition(_origin + _right * offset);
            _previousOffset = offset;
        }

        public void Stop()
        {
            _stopped = true;
            SetDirection(0);
        }

        private void SetDirection(int direction)
        {
            if (Direction == direction) return;
            Direction = direction;
            DirectionChanged?.Invoke(direction);
        }

        private void OnValidate()
        {
            _halfRange = Mathf.Max(0f, _halfRange);
            _speed = Mathf.Max(0f, _speed);
        }
    }
}
