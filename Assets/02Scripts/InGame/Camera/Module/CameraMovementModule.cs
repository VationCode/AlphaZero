using alpha.input;
using UnityEngine;

namespace alpha.camera
{
    public class CameraMovementModule : MonoBehaviour
    {
        #region Ref Component
        private AlphaInputSystem m_inputBoundary;
        #endregion

        #region Config 
        [SerializeField]
        private Transform m_camera;
        [SerializeField]
        private float m_followSpeed = 100;
        [Tooltip("감도"),SerializeField]
        private float m_sensitivity = 15;
        [Tooltip("각도 제한"), SerializeField]
        private float m_clampAngle = 70;

        [SerializeField]
        private float m_minDistance;
        [SerializeField]
        private float m_maxDistance;
        private float smoothness = 10f;
        [Space(10)]
        #endregion

        #region Runtime
        [SerializeField]
        private Transform m_currentTarget;

        private float m_currentX;
        private float m_currentY;
        private Vector3 m_dirNormalized;
        private Vector3 m_currentDir;
        private float m_currentDistance;
        #endregion

        public void Bind(AlphaInputSystem p_inputBoundary)
        {
            m_inputBoundary = p_inputBoundary;
        }
        private void Start()
        {
            if (m_currentTarget == null)
            {
                Debug.LogError("CameraMovementModule : Target is not set.");
                return;
            }

            m_currentX = transform.localRotation.eulerAngles.x;
            m_currentY = transform.localRotation.eulerAngles.y;

            m_dirNormalized = m_camera.localPosition.normalized;

            transform.position = m_currentTarget.position;
        }

        public void SetTarget(Transform target)
        {
            m_currentTarget = target;
        }

        public void Follow()
        {
            transform.position = Vector3.MoveTowards(transform.position, m_currentTarget.position, m_followSpeed * Time.deltaTime);

            // 로컬에서 월드좌표로 바꿔줌
            m_currentDir = transform.TransformPoint(m_dirNormalized * m_maxDistance);

            // 카메라와 타겟 사이에 장애물이 있는지 체크
            RaycastHit _hit;
            if (Physics.Linecast(transform.position, m_currentDir, out _hit))
            {
                m_currentDistance = Mathf.Clamp(_hit.distance, m_minDistance, m_maxDistance);
            }
            else
            {
                m_currentDistance = m_maxDistance;
            }

            m_camera.localPosition = Vector3.Lerp(m_camera.localPosition, m_dirNormalized * m_currentDistance, Time.deltaTime * smoothness);
        }
        public void Rotation()
        {
            m_currentX -= m_inputBoundary.LookInputDir.y * m_sensitivity * Time.deltaTime;
            m_currentY += m_inputBoundary.LookInputDir.x * m_sensitivity * Time.deltaTime;

            m_currentX = Mathf.Clamp(m_currentX, -m_clampAngle, m_clampAngle);

            Quaternion _rot = Quaternion.Euler(m_currentX, m_currentY, 0);
            transform.rotation = _rot;
        }

        private void Update()
        {
            Rotation();
        }
        
        private void LateUpdate()
        {
            Follow();
        }
    }
}