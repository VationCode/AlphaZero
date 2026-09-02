using alpha.input;
using System;
using UnityEngine;

namespace alpha.camera
{
    [Serializable]
    public struct ViewSettings
    {
        public float PivotHeight;
        public Vector3 PivotAngle;
        public float Shoulder;
        public float ZoomMinDistance;
        public float ZoomMaxDistance;
        public float RigFollowSpeed;
    }

    public class CameraMovementModule : MonoBehaviour
    {
        private AlphaInputSystem _input;

        [Header("[Object Ref]")]
        [Tooltip("타겟 추적"),SerializeField]
        private Transform _rig;
        [Tooltip("높이, 각도"), SerializeField]
        private Transform _pivot;
        [Tooltip("좌우"), SerializeField]
        private Transform _shoulder;
        [Tooltip("거리"), SerializeField]
        private Transform _zoom;
        [SerializeField]
        private Camera _camera;

        [Header("[Value Settings]")]
        [SerializeField]
        private ViewSettings[] _viewSettings;

        [Header("[Camera Collision]")]
        [SerializeField]
        private LayerMask _cameraCollisionMask;
        [SerializeField]
        private float _cameraCollisionRadius = 0.2f;
        [SerializeField]
        private float _cameraCollisionPadding = 0.1f;

        [Tooltip("감도"),SerializeField]
        private float m_sensitivity = 15;

        [Tooltip("각도 제한"), SerializeField]
        private float m_clampAngle = 70;

        private float smoothness = 10f;
        [Space(10)]

        [SerializeField]
        private Transform _target;

        private float _currentX;
        private float _currentY;

        public void Bind(AlphaInputSystem p_input)
        {
            _input = p_input;

            Reset();
        }

        private void Start()
        {
            if (_target == null)
            {
                Debug.LogError("CameraMovementModule : Target is not set.");
                return;
            }
        }

        private void Reset()
        {
            _rig.position = _target.position;
            _pivot.localPosition = new Vector3(0, _viewSettings[0].PivotHeight,0);
            _pivot.localEulerAngles = _viewSettings[0].PivotAngle;
            _shoulder.localPosition = new Vector3(_viewSettings[0].Shoulder, 0, 0);
            _zoom.localPosition = new Vector3(0, 0, -_viewSettings[0].ZoomMaxDistance);
            _camera.transform.localPosition = Vector3.zero;
        }

        private void RigFollow()
        {
            Vector3 lerp = Vector3.Lerp(_rig.position, _target.position, _viewSettings[0].RigFollowSpeed * Time.deltaTime);

            _rig.transform.position = lerp;
        }

        private void Rotation()
        {
            _currentX += _input.LookInputDir.x * m_sensitivity;
            _currentY -= _input.LookInputDir.y * m_sensitivity;

            _currentY = Mathf.Clamp(_currentY, -m_clampAngle, m_clampAngle);

            _pivot.localEulerAngles = new Vector3(_currentY, _currentX, 0);
        }

        private void Zoom()
        {
            float maxDistance = _viewSettings[0].ZoomMaxDistance;
            float minDistance = _viewSettings[0].ZoomMinDistance;

            Transform zoomParent = _zoom.parent;

            Vector3 origin = zoomParent.position;
            Vector3 direction = -zoomParent.forward;

            float targetDistance = maxDistance;

            if (Physics.SphereCast(origin, _cameraCollisionRadius, direction, out RaycastHit hit, maxDistance, _cameraCollisionMask))
            {
                targetDistance = Mathf.Clamp(hit.distance - _cameraCollisionPadding, minDistance, maxDistance);
            }

            Vector3 targetPosition = Vector3.back * targetDistance;

            _zoom.localPosition = Vector3.Lerp(_zoom.localPosition, targetPosition, smoothness * Time.deltaTime);
        }

        private void Update()
        {
            Rotation();
        }
        
        private void LateUpdate()
        {
            RigFollow();
            Zoom();
        }
    }
}