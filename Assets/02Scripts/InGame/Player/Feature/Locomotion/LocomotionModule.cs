// Module : 기능 연산 위주
using System;
using UnityEngine;

namespace alpha.player.locomotion
{
    [Serializable]
    public struct MovementSettings
    {
        [Header("Move Speed")]
        public float NormalMoveSpeed;
        public float SprintMoveSpeed;
        public float CombatMoveSpeed;
        //public float MoveAcceleration;
        //public float MoveDeceleration;

        [Header("Rotation")]
        public float RotationSmoothTime;
    }

    // 플레이어의 이동과 관련된 기능들을 담당하는 모듈
    public class LocomotionModule : MonoBehaviour
    {
        private LocomotionContext _locomotionContext;

        [SerializeField]
        private CharacterController _characterContrl;
        [SerializeField]
        private Transform _playerTransform;

        [SerializeField] // 0: Ground, 1: Flight, 2: Swimming, 3: Climbing
        private MovementSettings[] _movementSettings;

        private float _currentMoveSpeed;
        private Vector3 _rotationVelocity;
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

        public Vector3 MoveVelocity(Vector2 p_inputDir, bool p_isSprint, bool p_isCombat = false)
        {
            _currentMoveSpeed = p_isCombat ? _movementSettings[0].CombatMoveSpeed : _movementSettings[0].NormalMoveSpeed;

            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;

            Vector3 moveDir = (forward * p_inputDir.y) + (right * p_inputDir.x);

            moveDir.y = 0f;

            moveDir.Normalize();

            float targetSpeed = p_isSprint ? _movementSettings[0].SprintMoveSpeed : _currentMoveSpeed;

            Vector3 moveVelocity = moveDir * targetSpeed;

            _locomotionContext.SetMoveVelocity(moveVelocity);
            _locomotionContext.SetMoveSpeed(targetSpeed);

            return moveVelocity;
        }

        public void FinalMoveDirection(Vector3 p_moveVelocity)
        {
            _characterContrl.Move(p_moveVelocity * Time.deltaTime);
        }

        public void Rotation(Vector2 p_inputMove)
        {
            if (p_inputMove.sqrMagnitude < 0.01f)
                return;

            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 direction = (forward * p_inputMove.y) + (right * p_inputMove.x);

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            float angle = 
                Mathf.SmoothDampAngle(_playerTransform.eulerAngles.y, targetAngle, ref _rotationVelocity.y, _movementSettings[0].RotationSmoothTime);

            _playerTransform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
}