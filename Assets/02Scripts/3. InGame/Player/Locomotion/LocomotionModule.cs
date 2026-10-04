using System;
using UnityEngine;
namespace alpha.ingame.player
{
    public enum EMoveType
    {
        Idle = 0,
        Walk = 1,
        Jog = 2,
        Sprint = 3,
        Combat = 4
    }

    public class LocomotionModule : MonoBehaviour
    {
        [Header("Ref")]
        [SerializeField] private CharacterController _characterCtrl;

        [Header("Move")]
        [SerializeField] private float _walkSpeed = 2f;
        [SerializeField] private float _jogSpeed = 4f;
        [SerializeField] private float _sprintSpeed = 6f;
        [SerializeField] private float _combatSpeed = 3f;

        [Header("Speed Transition")]
        [Min(0.001f)]
        [SerializeField] private float _speedSmoothTime = 0.12f;

        [Header("Rotation")]
        [SerializeField] private float _rotationSpeed = 800f;

        [Header("Gravity")]
        [SerializeField] private float _gravity = -20f;

        public Vector3 MoveDirection { get; private set; }
        public float CurrentSpeed { get; private set; }
        public EMoveType MoveType { get; private set; } = EMoveType.Idle;
        public EMoveType PrevMoveType { get; private set; } = EMoveType.Idle;

        public event Action<Vector3, EMoveType, float> OnMove;

        private Vector3 _velocityXZ;
        private float _velocityY;

        private void Awake()
        {
            _characterCtrl = GetComponentInParent<CharacterController>(true);
        }
        public void SetMoveType(Vector2 p_input, bool p_isSprint, bool p_isWalk, bool p_isCombat)
        {
            // Combat Tree는 입력이 없어도 중앙의 대기 애니메이션을 사용한다.
            MoveType = p_isCombat ? EMoveType.Combat
                     : p_input.sqrMagnitude <= 0.0001f ? EMoveType.Idle
                     : p_isSprint ? EMoveType.Sprint
                     : p_isWalk ? EMoveType.Walk : EMoveType.Jog;
        }

        public void SetPrevMoveType(EMoveType p_moveType)
        {
            PrevMoveType = p_moveType;
        }

        // 캐릭터 반대 방향 입력시
        public bool IsOppositeDirection(Vector2 p_input)
        {
            Vector3 desiredDirection = GetWorldMoveDirection(p_input);
            if (desiredDirection.sqrMagnitude <= 0.0001f)
                return false;

            // Move() 호출 전 캐릭터가 실제로 바라보는 방향
            Vector3 facingDirection = _characterCtrl.transform.forward;
            facingDirection.y = 0f;

            return Vector3.Angle(facingDirection, desiredDirection) >= 175f;
        }

        public void Move(Vector2 p_input)
        {
            // 우선순위 유지: Combat > Sprint > Walk > Jog
            float targetSpeed = MoveType switch
            {
                EMoveType.Idle => 0f,
                EMoveType.Walk => _walkSpeed,
                EMoveType.Jog => _jogSpeed,
                EMoveType.Sprint => _sprintSpeed,
                EMoveType.Combat => _combatSpeed,
                _ => 0f
            };

            // 이동 속도 값을 부드럽게 전환
            CurrentSpeed = Mathf.Lerp(CurrentSpeed, targetSpeed, Time.deltaTime / _speedSmoothTime);

            // 카메라 기준 이동 방향을 저장해 애니메이션에도 전달
            MoveDirection = GetWorldMoveDirection(p_input);

            _velocityXZ = MoveDirection * targetSpeed;

            Rotation(MoveDirection, MoveType == EMoveType.Combat);

            Vector3 velocity = _velocityXZ;
            velocity.y = CalculateGravity(Time.deltaTime);
            _characterCtrl.Move(velocity * Time.deltaTime);

            // 이동 타입과 월드 방향을 애니메이션에 전달한다.
            OnMove?.Invoke(MoveDirection, MoveType, CurrentSpeed);
        }
        private Vector3 GetWorldMoveDirection(Vector2 p_input)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null || p_input.sqrMagnitude <= 0.0001f) return Vector3.zero;

            Vector3 forward = mainCamera.transform.forward;
            Vector3 right = mainCamera.transform.right;

            // 카메라의 위아래 기울기 제외
            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            //Vector2 input = Vector2.ClampMagnitude(p_input, 1f);

            return ((forward * p_input.y) + (right * p_input.x)).normalized; // 현재는 입력량으로 하는게 없기에 1로 정규화
        }

        public void Rotation(Vector3 p_direction, bool p_isimmediately = false)
        {
            if (_characterCtrl == null) return;

            Transform playerTransform = _characterCtrl.transform;

            // 즉시 카메라 정면 방향으로 회전
            if (p_isimmediately)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera == null) return;
                playerTransform.rotation = Quaternion.Euler(0f, mainCamera.transform.eulerAngles.y, 0f);
                return;
            }

            // 일반 이동 회전
            p_direction.y = 0f;
            if (p_direction.sqrMagnitude <= 0.0001f)
                return;

            // 일반 이동 중에는 목표 이동 방향으로 회전
            Quaternion targetRotation = Quaternion.LookRotation(p_direction.normalized, Vector3.up);

            playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
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
