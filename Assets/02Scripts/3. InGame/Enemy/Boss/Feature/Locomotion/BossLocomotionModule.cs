using UnityEngine;

namespace alpha.ingame.enemy.boss
{
    public enum EBossMovePace
    {
        Walk,
        Run
    }

    /// <summary>보스의 물리 이동, 회전, 점프를 실행한다. 호출자는 실행 시점과 목적지를 결정한다.</summary>
    [DisallowMultipleComponent]
    public sealed class BossLocomotionModule : MonoBehaviour
    {
        [Header("Ground Movement")]
        [SerializeField, Min(0f)] private float _walkSpeed = 2f;
        [SerializeField, Min(0f)] private float _runSpeed = 5f;
        [SerializeField, Min(0f)] private float _turnSpeed = 180f;
        [SerializeField, Range(0f, 180f)] private float _facingTolerance = 5f;

        private Rigidbody _body;
        private Vector3 _jumpStart;
        private Vector3 _jumpDestination;
        private float _jumpProgress;
        private bool _restoreGravity;
        private Vector3 _buttStart;
        private Vector3 _buttForward;
        private float _buttDistance;
        private float _buttProgress;
        private RigidbodyConstraints _constraintsBeforeRecovery;
        private bool _recoveryRotationLocked;

        public bool CanMove => isActiveAndEnabled && _body != null && !_body.isKinematic;
        public Vector3 Position => _body != null ? _body.position : transform.root.position;
        public Vector3 Forward => _body != null ? _body.rotation * Vector3.forward : transform.root.forward;
        public float CurrentSpeed => _body != null ?
            Vector3.ProjectOnPlane(_body.linearVelocity, Vector3.up).magnitude : 0f;
        public bool IsJumping { get; private set; }
        public bool IsButting { get; private set; }

        public void Bind(Rigidbody body)
        {
            UnlockRecoveryRotation();
            Stop();
            _body = body;
        }

        // 물리 프레임에서 반복 호출한다. 도착하면 수평 이동을 멈춘다.
        public bool MoveTo(Vector3 destination, EBossMovePace pace, float stopDistance)
        {
            if (_body == null || _body.isKinematic || IsJumping || IsButting) return false;

            Vector3 offset = destination - _body.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            float stoppingDistance = Mathf.Max(0f, stopDistance);
            if (distance <= stoppingDistance)
            {
                StopGroundMovement();
                return true;
            }

            float configuredSpeed = pace == EBossMovePace.Run ? _runSpeed : _walkSpeed;
            float speed = Mathf.Min(configuredSpeed,
                (distance - stoppingDistance) / Time.fixedDeltaTime);
            Vector3 velocity = _body.linearVelocity;
            Vector3 direction = offset / distance;
            velocity.x = direction.x * speed;
            velocity.z = direction.z * speed;
            _body.linearVelocity = velocity; // 중력에 따른 Y 속도는 유지한다.
            return false;
        }

        public bool IsFacing(Vector3 targetPosition)
        {
            if (_body == null || _body.isKinematic) return false;

            Vector3 direction = targetPosition - _body.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return true;

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            return Quaternion.Angle(_body.rotation, targetRotation) <= _facingTolerance;
        }

        // 물리 프레임에서 회전한다. 실제 회전이 적용된 다음 틱에 정면을 확인한다.
        public bool Face(Vector3 targetPosition)
        {
            if (_recoveryRotationLocked || IsButting) return false;
            if (IsFacing(targetPosition)) return true;
            if (_body == null || _body.isKinematic) return false;

            Vector3 direction = targetPosition - _body.position;
            direction.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            Quaternion next = Quaternion.RotateTowards(_body.rotation, targetRotation,
                _turnSpeed * Time.fixedDeltaTime);
            _body.MoveRotation(next);
            return false;
        }

        // 보스의 최초 정면처럼 위치와 무관한 월드 방향으로 회전한다.
        public bool FaceForward(Vector3 worldForward)
        {
            if (_recoveryRotationLocked || IsButting || _body == null ||
                _body.isKinematic || IsJumping) return false;

            Vector3 direction = Vector3.ProjectOnPlane(worldForward, Vector3.up);
            if (direction.sqrMagnitude < 0.0001f) return false;
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            if (Quaternion.Angle(_body.rotation, targetRotation) <= 0.01f) return true;
            Quaternion next = Quaternion.RotateTowards(_body.rotation, targetRotation,
                _turnSpeed * Time.fixedDeltaTime);
            _body.MoveRotation(next);
            return false;
        }

        // 공격 후 대기 중에는 물리 충돌에도 방향이 바뀌지 않게 한다.
        public void LockRecoveryRotation()
        {
            if (_body == null || _recoveryRotationLocked) return;

            _constraintsBeforeRecovery = _body.constraints;
            if (!_body.isKinematic) _body.angularVelocity = Vector3.zero;
            _body.constraints |= RigidbodyConstraints.FreezeRotationY;
            _recoveryRotationLocked = true;
        }

        public void UnlockRecoveryRotation()
        {
            if (!_recoveryRotationLocked) return;
            if (_body != null) _body.constraints = _constraintsBeforeRecovery;
            _recoveryRotationLocked = false;
        }

        // JumpStart에서 착지점을 고정하고 클립의 이벤트 진행률로 이동한다.
        public bool BeginJumpTo(Vector3 destination)
        {
            if (_body == null || _body.isKinematic || IsJumping || IsButting) return false;

            StopGroundMovement();
            _jumpStart = _body.position;
            _jumpDestination = destination;
            _jumpProgress = 0f;
            _restoreGravity = _body.useGravity;
            _body.linearVelocity = Vector3.zero;
            _body.useGravity = false;
            IsJumping = true;
            return true;
        }

        public void SetJumpProgress(float progress)
        {
            if (IsJumping)
                _jumpProgress = Mathf.Max(_jumpProgress, Mathf.Clamp01(progress));
        }

        // ButtStart 시점의 위치와 방향을 고정하고 설정 거리만큼 전진한다.
        public bool BeginButtMotion(float distance)
        {
            if (_body == null || _body.isKinematic || IsJumping || IsButting ||
                _recoveryRotationLocked) return false;

            StopGroundMovement();
            _buttStart = _body.position;
            _buttForward = Vector3.ProjectOnPlane(Forward, Vector3.up).normalized;
            _buttDistance = Mathf.Max(0f, distance);
            _buttProgress = 0f;
            IsButting = true;
            return true;
        }

        public void SetButtProgress(float progress)
        {
            if (IsButting)
                _buttProgress = Mathf.Max(_buttProgress, Mathf.Clamp01(progress));
        }

        public void EndButtMotion()
        {
            if (!IsButting || _body == null) return;

            Vector3 destination = _buttStart + _buttForward * _buttDistance;
            destination.y = _body.position.y;
            _body.MovePosition(destination); // ButtEnd에서 설정 거리의 끝까지 도착한다.
            IsButting = false;
            StopGroundMovement();
        }

        private void FixedUpdate()
        {
            if (_body == null) return;

            if (IsButting)
            {
                Vector3 destination = _buttStart + _buttForward * (_buttDistance * _buttProgress);
                destination.y = _body.position.y; // Y축 중력은 물리에 맡긴다.
                _body.MovePosition(destination);
            }

            if (!IsJumping) return;

            // 높이는 모델의 MainJoint 애니메이션이 표현하고, Rigidbody는 착지점으로 이동한다.
            _body.MovePosition(Vector3.Lerp(_jumpStart, _jumpDestination, _jumpProgress));
        }

        // 착지 이벤트에서 남은 물리 프레임 오차를 정리한다.
        public void CompleteJump()
        {
            if (!IsJumping || _body == null) return;

            _body.position = _jumpDestination;
            IsJumping = false;
            _body.useGravity = _restoreGravity;
            _body.linearVelocity = Vector3.zero;
        }

        public void Stop()
        {
            IsButting = false;
            _buttDistance = 0f;
            _buttProgress = 0f;
            if (IsJumping)
            {
                IsJumping = false;
                if (_body != null) _body.useGravity = _restoreGravity;
            }

            StopGroundMovement();
        }

        private void StopGroundMovement()
        {
            if (_body == null || _body.isKinematic) return;
            Vector3 velocity = _body.linearVelocity;
            velocity.x = 0f;
            velocity.z = 0f;
            _body.linearVelocity = velocity;
        }

        private void OnDisable() => Stop();

        private void OnDestroy() => UnlockRecoveryRotation();

        private void OnValidate()
        {
            _walkSpeed = Mathf.Max(0f, _walkSpeed);
            _runSpeed = Mathf.Max(0f, _runSpeed);
            _turnSpeed = Mathf.Max(0f, _turnSpeed);
            _facingTolerance = Mathf.Clamp(_facingTolerance, 0f, 180f);
        }
    }
}
