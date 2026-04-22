using alpha.camera.module;
using alpha.player.boundary;
using alpha.player.module;
using UnityEngine;

namespace alpha.camera
{
    [RequireComponent(typeof(FollowModule))]
    public class CameraCore : MonoBehaviour
    {
        private InputSystemBoundary m_inputSystemBoundary;
        [SerializeField]
        private FollowModule m_followModule;

        private void Awake()
        {
            m_followModule = GetComponent<FollowModule>();
        }

        public void Bind(InputSystemBoundary inputSystemBoundary)
        {
            m_inputSystemBoundary = inputSystemBoundary;

            m_followModule.Bind(m_inputSystemBoundary);
        }
    }
}