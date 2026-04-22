using alpha.camera.module;
using alpha.player.boundary;
using alpha.player.module;
using UnityEngine;

namespace alpha.camera
{
    [RequireComponent(typeof(FollowModule))]
    public class CameraCore : MonoBehaviour
    {
        [SerializeField]
        private FollowModule m_followModule;

        private void Awake()
        {
            m_followModule = GetComponent<FollowModule>();
        }

        public void Bind(PlayerInputBoundary inputBoundary)
        {
            m_followModule.Bind(inputBoundary);

        }
    }
}