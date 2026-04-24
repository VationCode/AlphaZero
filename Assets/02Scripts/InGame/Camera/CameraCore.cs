using alpha.camera.module;
using alpha.player.boundary;
using alpha.player.module;
using UnityEngine;

namespace alpha.camera
{
    [RequireComponent(typeof(CameraMovementModule))]
    public class CameraCore : MonoBehaviour
    {
        private InputSystemBoundary m_inputSystemBoundary;
        [SerializeField]
        private CameraMovementModule m_movementModule;

        private void Awake()
        {
            m_movementModule = GetComponent<CameraMovementModule>();
        }

        public void Bind(InputSystemBoundary inputSystemBoundary)
        {
            m_inputSystemBoundary = inputSystemBoundary;

            m_movementModule.Bind(m_inputSystemBoundary);
        }
    }
}