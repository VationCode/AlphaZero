using alpha.player.boundary;
using UnityEngine;

namespace alpha.camera.module
{
    public class FollowModule : MonoBehaviour
    {
        #region Ref Component
        private InputSystemBoundary m_inputBoundary;
        #endregion

        #region Config 
        [SerializeField]
        private float m_rotSpeed;
        [SerializeField]
        private float m_followSpeed;
        [SerializeField]
        private float m_distance;

        #endregion

        #region Runtime
        [SerializeField]
        private Transform m_currentTarget;

        #endregion

        public void Bind(InputSystemBoundary inputBoundary)
        {
            m_inputBoundary = inputBoundary;
        }
        private void Start()
        {
            if (m_currentTarget == null)
                Debug.LogError("FollowModule : Target is not set.");
            else
                Follow();
        }

        public void SetTarget(Transform target)
        {
            m_currentTarget = target;
        }

        public void Follow()
        {
            Debug.Log(m_inputBoundary.LookInputDir);
           // transform.position = m_currentTarget.position + m_offset;
        }

        

        private void Update()
        {
            Follow();
        }
    }
}