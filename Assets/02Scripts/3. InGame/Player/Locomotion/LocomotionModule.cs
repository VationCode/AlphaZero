using System;
using UnityEngine;
namespace alpha.ingame.player
{
    public enum EMoveType
    {
        Idle = 0,
        Walk = 1,
        Run = 2,
        Combat = 3,
        Boost = 4,
        BoostCombat = 5
    }

    [DefaultExecutionOrder(100)]
    public class LocomotionModule : MonoBehaviour
    {
        [Header("Ref")]
        [SerializeField] private CharacterController _characterCtrl;

        [Header("Move")]
        [SerializeField] private float _walkSpeed = 2f;
        [SerializeField] private float _runSpeed = 6f;
        [SerializeField] private float _combatSpeed = 3f;

        [Header("Mobility")]
        [SerializeField, Min(0f)] private float _boostSpeed = 10f;
        [SerializeField, Min(0f)] private float _leapSpeed = 10f;
        [SerializeField, Min(0.01f)] private float _leapDuration = 0.18f;
        [SerializeField, Min(0.01f)] private float _leapHeight = 1.4f;

        [Header("Speed Transition")]
        [Min(0.001f)]
        [SerializeField] private float _speedSmoothTime = 0.12f;

        [Header("Rotation")]
        [SerializeField] private float _rotationSpeed = 800f;

        [Header("Air")]
        [SerializeField, Min(0.01f)] private float _jumpHeight = 2f;
        [SerializeField, Min(0f)] private float _airSpeed = 3f;
        [SerializeField, Min(0f)] private float _airAcceleration = 8f;

        [Header("Gravity")]
        [SerializeField] private float _gravity = -20f;

        public Vector3 MoveDirection { get; private set; }
        public float CurrentSpeed { get; private set; }
        public EMoveType MoveType { get; private set; } = EMoveType.Idle;
        public Transform LockOnTarget { get; private set; }

        // 월드 이동 방향, 이동 타입, 0~1 입력 세기
        public event Action<Vector3, EMoveType, float> OnMove;
        public event Action<int> OnJump;

        public void ResetJumpCount() => _jumpCount = 0;

        private Vector3 _velocityXZ;
        private Vector3 _leapDirection;
        public float VelocityY => _velocityY;
        private float _velocityY;
        private int _jumpCount;
        public bool IsGrounded => _characterCtrl.isGrounded;
        public bool IsFalling => !IsGrounded && _velocityY < 0f;
        public bool CanGroundJump => IsGrounded && _jumpCount == 0;
        public bool CanAirJump => !IsGrounded && _jumpCount == 1;
        public float LeapDuration => _leapDuration;
        private void Awake()
        {
            _characterCtrl = GetComponentInParent<CharacterController>(true);
        }

        private void LateUpdate()
        {
            // 전투 또는 락온 중에만 카메라/대상 회전을 계속 따라간다.
            if (MoveType == EMoveType.Combat || MoveType == EMoveType.BoostCombat || LockOnTarget != null)
                UpdateFacing(MoveDirection, true);
        }
        public void SetMoveType(Vector2 p_input, bool p_isWalk, bool p_isCombat)
        {
            // Combat Tree는 입력이 없어도 중앙의 대기 애니메이션을 사용한다.
            MoveType = p_isCombat ? EMoveType.Combat
                     : p_input.sqrMagnitude <= 0.0001f ? EMoveType.Idle
                     : p_isWalk ? EMoveType.Walk :  EMoveType.Run;
        }

        public void SetBoostMoveType(bool isCombat) =>
            MoveType = isCombat ? EMoveType.BoostCombat : EMoveType.Boost;

        public void Move(Vector2 p_input)
        {
            // 전투 이동을 우선하고, 걷기 입력에 따라 속도를 선택한다.
            float inputStrength = Mathf.Clamp01(p_input.magnitude);
            MoveDirection = GetWorldMoveDirection(p_input);

            float targetSpeed = MoveType switch
            {
                EMoveType.Idle => 0f,
                EMoveType.Walk => _walkSpeed,
                EMoveType.Run => _runSpeed,
                EMoveType.Combat => _combatSpeed,
                EMoveType.Boost => _boostSpeed,
                EMoveType.BoostCombat => _boostSpeed,
                _ => 0f
            };

            UpdateFacing(MoveDirection, MoveType == EMoveType.Combat || MoveType == EMoveType.BoostCombat);

            // 이동 속도 값을 부드럽게 전환
            CurrentSpeed = Mathf.Lerp(CurrentSpeed, targetSpeed * inputStrength, Time.deltaTime / _speedSmoothTime);

            _velocityXZ = MoveDirection * targetSpeed * inputStrength;

            Vector3 velocity = _velocityXZ;
            velocity.y = CalculateGravity(Time.deltaTime);
            _characterCtrl.Move(velocity * Time.deltaTime);

            // 월드 방향과 입력 세기를 애니메이션에 전달한다.
            OnMove?.Invoke(MoveDirection, MoveType, inputStrength);
        }

        private Vector3 GetWorldMoveDirection(Vector2 p_input)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null || p_input.sqrMagnitude <= 0.0001f)
                return Vector3.zero;

            Vector3 forward = mainCamera.transform.forward;
            Vector3 right = mainCamera.transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return (forward * p_input.y + right * p_input.x).normalized;
        }

        public void SetLockOnTarget(Transform target)
        {
            LockOnTarget = target;
            UpdateFacing(MoveDirection, MoveType == EMoveType.Combat);
        }

        private void UpdateFacing(Vector3 p_direction, bool p_faceCameraImmediately)
        {
            if (_characterCtrl == null) return;

            Transform playerTransform = _characterCtrl.transform;
            if (LockOnTarget != null)
            {
                Vector3 targetDirection = LockOnTarget.position - playerTransform.position;
                targetDirection.y = 0f;
                if (targetDirection.sqrMagnitude > 0.0001f)
                    playerTransform.rotation = Quaternion.LookRotation(targetDirection, Vector3.up);
                return;
            }

            if (p_faceCameraImmediately)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera == null) return;
                playerTransform.rotation = Quaternion.Euler(0f, mainCamera.transform.eulerAngles.y, 0f);
                return;
            }

            // 일반 이동은 실제 이동 방향으로 부드럽게 회전한다.
            p_direction.y = 0f;
            if (p_direction.sqrMagnitude <= 0.0001f) return;
            Quaternion targetRotation = Quaternion.LookRotation(p_direction, Vector3.up);
            playerTransform.rotation = Quaternion.RotateTowards(
                playerTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }

        public void Jump()
        {
            // 진입 가능 여부는 상태에서 확인한다.
            _velocityY = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            _jumpCount++;
            OnJump?.Invoke(_jumpCount);
        }

        public void BeginLeap(Vector2 p_moveInput)
        {
            // 입력이 없으면 카메라 정면으로 도약한다.
            _leapDirection = GetWorldMoveDirection(
                p_moveInput.sqrMagnitude > 0.0001f ? p_moveInput : Vector2.up);
            if (_leapDirection.sqrMagnitude <= 0.0001f)
                _leapDirection = _characterCtrl.transform.forward;

            MoveDirection = _leapDirection;
            _velocityXZ = _leapDirection * _leapSpeed;
            _velocityY = Mathf.Sqrt(_leapHeight * -2f * _gravity);
            _jumpCount = 1;
            UpdateFacing(_leapDirection, false);
            OnJump?.Invoke(_jumpCount);
        }

        public void LeapMove()
        {
            _velocityXZ = _leapDirection * _leapSpeed;
            CurrentSpeed = _leapSpeed;
            UpdateFacing(_leapDirection, false);

            Vector3 velocity = _velocityXZ;
            velocity.y = CalculateGravity(Time.deltaTime);
            _characterCtrl.Move(velocity * Time.deltaTime);
        }

        public void AirMove(Vector2 p_moveInput)
        {
            float deltaTime = Time.deltaTime;

            MoveDirection = GetWorldMoveDirection(p_moveInput);
            Vector3 targetVelocityXZ = MoveDirection * _airSpeed;
            _velocityXZ = Vector3.MoveTowards(_velocityXZ, targetVelocityXZ, _airAcceleration * deltaTime);

            CurrentSpeed = _velocityXZ.magnitude;
            UpdateFacing(MoveDirection, MoveType == EMoveType.Combat);

            // 공중에서도 중력과 CharacterController 이동을 매 프레임 처리한다.
            Vector3 velocity = _velocityXZ;
            velocity.y = CalculateGravity(deltaTime);
            CollisionFlags collision = _characterCtrl.Move(velocity * deltaTime);

            // 천장에 닿으면 상승을 종료한다.
            if ((collision & CollisionFlags.Above) != 0 && _velocityY > 0f)
                _velocityY = 0f;
        }

        public void HoldGround()
        {
            MoveDirection = Vector3.zero;
            _velocityXZ = Vector3.zero;
            CurrentSpeed = 0f;

            // OnMove를 발생시키지 않아 착지 애니메이션을 유지한다.
            _velocityY = -2f;
            _characterCtrl.Move(Vector3.up * (_velocityY * Time.deltaTime));
        }

        // 수직 속도 계산
        private float CalculateGravity(float p_deltaTime)
        {
            if (_characterCtrl.isGrounded && _velocityY < 0f)
                _velocityY = -2f;

            _velocityY += _gravity * p_deltaTime;
            return _velocityY;
        }
    }
}
