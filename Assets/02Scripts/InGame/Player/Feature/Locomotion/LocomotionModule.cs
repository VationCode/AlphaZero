// Module : 기능 연산 위주
using System;
using UnityEngine;

namespace alpha.player.locomotion
{
    [Serializable]
    public struct MovementSettings
    {
        public string Name;
        [Header("[Move Speed]")]
        public float NormalMoveSpeed;
        public float SprintMoveSpeed;
        public float CombatMoveSpeed;

        [Header("Rotation")]
        public float RotationSmoothTime;
    }

    [Serializable]
    public struct EvasionSettings       // 회피 관련 설정(Dash, Dodge)
    {
        public float Distance;

        public float Duration;
    }

    // 플레이어의 이동과 관련된 기능들을 담당하는 모듈
    public class LocomotionModule : MonoBehaviour
    {
        private LocomotionContext _locomotionContext;

        [SerializeField]
        private CharacterController _characterContrl;
        [SerializeField]
        private Transform _playerTransform;

        [Header("[Move]"), SerializeField] // 0: Ground, 1: Flight, 2: Swimming, 3: Climbing
        private MovementSettings[] _movementSettings;

        [Header("[Gravity]")]
        [SerializeField]
        private float _gravity;

        [Header("[Ground]")]
        [SerializeField]
        private Vector3 _checkBoxHalfSize;
        [Range(-1f, 1f), SerializeField]
        private float _boundMinYOffset = 0.09f;
        [SerializeField]
        private LayerMask _groundLayer;

        [Header("[Jump]")]
        [SerializeField]
        private EvasionSettings _jumpSetting;

        [Header("[Dash]")]
        [SerializeField]
        private EvasionSettings _dashSettings;

        [Header("[Dodge]")]
        [SerializeField]
        private EvasionSettings _dodgeSettings;

        public float DashDuration => _dashSettings.Duration;
        public float DodgeDuration => _dodgeSettings.Duration;

        private float _currentMoveSpeed;
        private Vector3 _rotationVelocity;

        private Vector3 _currentVelocity;
        private float _currentVerticalVelocity;

        private bool _isGrounded;


        private void Awake()
        {
            if (_characterContrl == null)
            {
                _characterContrl = GetComponentInParent<CharacterController>();
            }
        }
        
        public void Bind(LocomotionContext p_context)
        {
            _locomotionContext = p_context;
        }

        private void Update()
        {
            CheckGrounded();
            ApplyGravity();
        }

        #region ============================== Gravity
        private void ApplyGravity()
        {
            _currentVerticalVelocity += _gravity * Time.deltaTime;

            if(_currentVerticalVelocity < 0f && _isGrounded)
            {
                _currentVerticalVelocity = -2f;
            }

            _locomotionContext.SetVerticalVelocity(_currentVerticalVelocity);
        }
        #endregion ============================== Gravity

        #region ============================== Ground
        private void CheckGrounded()
        {
            Vector3 bottom = GetGroundCheckPosition();

            Vector3 halfExtents = _checkBoxHalfSize;

            // Sphere를 사용하지 않는 이유는 떨어질 구간에서 뒷쪽이 걸려버리는 상황이 발생하므로
            if (Physics.CheckBox(bottom, halfExtents, Quaternion.identity, _groundLayer))
            {
                _isGrounded = true;
            }
            else
            {
                _isGrounded = false;
            }

            _locomotionContext.SetGrounded(_isGrounded);
        }
        private Vector3 GetGroundCheckPosition()
        {
            Bounds bounds = _characterContrl.bounds;

            return new Vector3(bounds.center.x, bounds.min.y + _boundMinYOffset, bounds.center.z);
        }

        private void OnDrawGizmosSelected()
        {
            if (_characterContrl == null)
                return;

            Vector3 bottom = GetGroundCheckPosition();

            Vector3 halfExtents = _checkBoxHalfSize;


            Gizmos.DrawWireCube(bottom, halfExtents);
        }
        #endregion ============================== Ground


        public Vector3 MoveVelocity(Vector2 p_inputDir, bool p_isSprint, bool p_isCombat = false)
        {
            _currentMoveSpeed = p_isCombat ? _movementSettings[0].CombatMoveSpeed : _movementSettings[0].NormalMoveSpeed;

            Vector3 moveDir = GetMoveDirection(p_inputDir);

            float targetSpeed = p_isSprint ? _movementSettings[0].SprintMoveSpeed : _currentMoveSpeed;

            Vector3 moveVelocity = moveDir * targetSpeed;

            if (moveDir.sqrMagnitude > 0.01f)
            {
                _locomotionContext.SetLastMoveInput(p_inputDir);
                _locomotionContext.SetLastMoveDirection(moveDir);
            }

            _locomotionContext.SetHorizontalVelocity(moveVelocity);

            return moveVelocity;
        }
        public Vector3 GetForwardDirection()
        {
            return _playerTransform.forward;
        }

        // 월드
        public Vector3 GetMoveDirection(Vector2 p_inputDir)
        {
            if (p_inputDir.sqrMagnitude < 0.01f)
                return Vector3.zero;

            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 direction = (forward * p_inputDir.y) + (right * p_inputDir.x);

            return direction.normalized;
        }

        // 로컬
        public Vector2 GetLocalDirection(Vector3 p_worldDirection)
        {
            Vector3 localDirection = _playerTransform.InverseTransformDirection(p_worldDirection);

            return new Vector2(localDirection.x, localDirection.z);
        }

        public void FinalMoveDirection(Vector3 p_moveVelocity)
        {
            _characterContrl.Move(p_moveVelocity * Time.deltaTime);
        }

        public void Rotation(Vector2 p_inputMove)
        {
            Vector3 direction = GetMoveDirection(p_inputMove);

            if (direction.sqrMagnitude < 0.01f)
                return;

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            float angle = 
                Mathf.SmoothDampAngle(_playerTransform.eulerAngles.y, targetAngle, ref _rotationVelocity.y, _movementSettings[0].RotationSmoothTime);

            _playerTransform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        // 즉시 회전
        public void RotationImmediate(Vector3 p_worldDirection)
        {
            if (p_worldDirection.sqrMagnitude < 0.01f)
                return;

            p_worldDirection.y = 0f;

            _playerTransform.rotation =
                Quaternion.LookRotation(p_worldDirection.normalized);

            // 기존 SmoothDamp 회전 속도값 제거
            _rotationVelocity = Vector3.zero;
        }

        public void Jump()
        {
            // Jump 기능 구현
        }

        public void Fall()
        {

        }

        public void Land()
        {
            // Land 기능 구현
        }

        public void Dash(Vector3 p_direction)
        {
            float dashSpeed = _dashSettings.Distance / _dashSettings.Duration;

            Vector3 velocity = p_direction.normalized * dashSpeed;

            velocity.y = _currentVerticalVelocity;

            FinalMoveDirection(velocity);
        }

        public void Dodge(Vector3 p_moveDir)
        {
            if (p_moveDir.sqrMagnitude < 0.01f)
                p_moveDir = _playerTransform.forward;

            float dodgeSpeed = _dodgeSettings.Distance / _dodgeSettings.Duration;

            Vector3 velocity = p_moveDir.normalized * dodgeSpeed;
            velocity.y = _currentVerticalVelocity;

            FinalMoveDirection(velocity);
        }
    }
}